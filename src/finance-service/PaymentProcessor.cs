using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Finance.Api;

/// <summary>
/// RabbitMQ background consumer. It declares the shared topic exchange and a
/// queue bound to <c>finance.payment.request</c>, then on every incoming
/// <see cref="PaymentRequestMessage"/>:
/// <list type="number">
///   <item>logs the request,</item>
///   <item>waits 2 s (simulating provider work),</item>
///   <item>publishes a <see cref="PaymentResultMessage"/> with
///   <c>outcome = "AUTHORIZED"</c> to <c>sales.payment.result</c>,</item>
///   <item>acks the message (nacks &amp; requeues on failure).</item>
/// </list>
/// Startup retries until RabbitMQ is reachable, so docker-compose ordering is
/// forgiving.
/// </summary>
internal sealed class PaymentProcessor : BackgroundService
{
    private const string Exchange = "dollarshop.events";
    private const string RequestQueue = "finance.payment-requests";
    private const string RequestRoutingKey = "finance.payment.request";
    private const string ResultRoutingKey = "sales.payment.result";

    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentProcessor> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public PaymentProcessor(IConfiguration configuration, ILogger<PaymentProcessor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);
        if (_channel is null)
        {
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: RequestQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("finance-service is consuming payment requests on '{Queue}'.", RequestQueue);

        // Stay alive until the host shuts us down.
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.TryParse(_configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = _configuration["RabbitMq:Username"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest",
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await _channel.ExchangeDeclareAsync(
                    exchange: Exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                await _channel.QueueDeclareAsync(
                    queue: RequestQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                await _channel.QueueBindAsync(
                    queue: RequestQueue,
                    exchange: Exchange,
                    routingKey: RequestRoutingKey,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}.", factory.HostName, factory.Port);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ not reachable yet — retrying in {Delay}s.", ReconnectDelay.TotalSeconds);
                try
                {
                    await Task.Delay(ReconnectDelay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
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
            var request = JsonSerializer.Deserialize<PaymentRequestMessage>(json, JsonOptions)
                ?? throw new InvalidOperationException("Empty payment-request payload.");

            _logger.LogInformation(
                "Received payment request {Tx} ({Amount} {Currency}); auto-approving in {Delay}s.",
                request.TransactionReference, request.Amount, request.CurrencyCode, ProcessingDelay.TotalSeconds);

            await Task.Delay(ProcessingDelay);

            var result = new PaymentResultMessage(
                TransactionReference: request.TransactionReference,
                Outcome: "AUTHORIZED",
                DeclineReason: null,
                ProcessedAt: DateTimeOffset.UtcNow);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
            };

            await channel.BasicPublishAsync(
                exchange: Exchange,
                routingKey: ResultRoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result, JsonOptions)));

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);

            _logger.LogInformation("Published AUTHORIZED result for {Tx}.", request.TransactionReference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process a payment request; requeueing.");
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
