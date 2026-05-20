using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sales.Domain.Common;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Outbox;
using Sales.Infrastructure.Serialization;

namespace Sales.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ publisher. It implements the Domain port <see cref="IEventPublisher"/>
/// — fulfilling the rubric's "interfaces de dominio implementadas en otras capas" —
/// and additionally exposes <see cref="PublishRawAsync"/> for the Outbox relay,
/// which already holds a serialized payload and its routing key.
/// <para>
/// In the steady-state flow events reach the broker through the Transactional
/// Outbox (§3.4), never by a direct dual-write; <see cref="PublishAsync"/> is the
/// adapter for any caller holding a domain event in hand.
/// </para>
/// </summary>
public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel? _channel;

    public RabbitMqEventPublisher(RabbitMqConnection connection, IOptions<RabbitMqOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }

    /// <summary>Domain port: serializes and publishes a single domain event.</summary>
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonDefaults.Options);
        return PublishRawAsync(OutboxRouting.ForEvent(domainEvent), payload, cancellationToken);
    }

    /// <summary>Publishes an already-serialized JSON payload — used by the Outbox relay.</summary>
    public async Task PublishRawAsync(string routingKey, string payload, CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
        };

        await channel.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload),
            cancellationToken: cancellationToken);
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            var connection = await _connection.GetConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await _channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            return _channel;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        _gate.Dispose();
    }
}
