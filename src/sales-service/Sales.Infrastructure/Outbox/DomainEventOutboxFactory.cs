using System.Text.Json;
using Sales.Domain.Common;
using Sales.Infrastructure.Persistence;
using Sales.Infrastructure.Serialization;

namespace Sales.Infrastructure.Outbox;

/// <summary>
/// Turns a recorded <see cref="IDomainEvent"/> into an <see cref="OutboxMessage"/>
/// row: it serializes the event to JSON and derives its routing key. Used by
/// <c>SalesDbContext.CommitAsync</c> while draining the aggregates' events into
/// the Outbox within the business transaction.
/// </summary>
internal static class DomainEventOutboxFactory
{
    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent) =>
        OutboxMessage.Create(
            messageType: domainEvent.GetType().Name,
            routingKey: OutboxRouting.ForEvent(domainEvent),
            payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonDefaults.Options),
            occurredOn: domainEvent.OccurredOn);
}
