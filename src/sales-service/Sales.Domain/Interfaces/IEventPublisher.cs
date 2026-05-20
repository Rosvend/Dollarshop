using Sales.Domain.Common;

namespace Sales.Domain.Interfaces;

/// <summary>
/// Dispatch contract for domain events. Declared in the Domain layer and
/// implemented in Infrastructure (e.g. a RabbitMQ publisher), keeping the
/// messaging technology out of the domain.
/// </summary>
public interface IEventPublisher
{
    /// <summary>Publishes a single domain event.</summary>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
