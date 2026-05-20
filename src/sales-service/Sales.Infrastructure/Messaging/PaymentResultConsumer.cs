using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Infrastructure.Acl;
using Sales.Infrastructure.Acl.Contracts;
using Sales.Infrastructure.Serialization;

namespace Sales.Infrastructure.Messaging;

/// <summary>
/// Inbound side of the Finance ACL: a background worker that consumes Finance's
/// <c>PaymentResult</c> messages from RabbitMQ, runs them through
/// <see cref="FinanceAclMapper"/>, and dispatches the resulting Sales integration
/// event (<c>PagoAprobado</c> / <c>PagoRechazado</c>) through MediatR — driving the
/// Application-layer handlers that continue or compensate the checkout Saga.
/// <para>
/// Messages are manually acknowledged: a handled message is acked, a failed one is
/// nacked and requeued. The Application handlers are idempotent, which is required
/// under the broker's at-least-once delivery (§3.4).
/// </para>
/// </summary>
public sealed class PaymentResultConsumer : BackgroundService
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(10);

    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentResultConsumer> _logger;
    private IChannel? _channel;

    public PaymentResultConsumer(
        RabbitMqConnection connection,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentResultConsumer> logger)
    {
        _connection = connection;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumingAsync(stoppingToken);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment-result consumer failed; reconnecting shortly.");
                try
                {
                    await Task.Delay(ReconnectDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var connection = await _connection.GetConnectionAsync(cancellationToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _options.PaymentResultQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: _options.PaymentResultQueue,
            exchange: _options.Exchange,
            routingKey: _options.PaymentResultRoutingKey,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: _options.PaymentResultQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Listening for Finance payment results on queue {Queue}.", _options.PaymentResultQueue);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var channel = _channel;
        if (channel is null)
        {
            return;
        }

        try
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var result = JsonSerializer.Deserialize<PaymentResultMessage>(json, JsonDefaults.Options)
                ?? throw new InvalidOperationException("Empty payment-result payload.");

            // ACL translation: Finance's vocabulary -> a Sales integration event.
            var notification = FinanceAclMapper.ToNotification(result);

            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(notification);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process a Finance payment result; requeueing.");
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }
    }
}
