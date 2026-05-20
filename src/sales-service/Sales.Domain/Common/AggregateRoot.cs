namespace Sales.Domain.Common;

/// <summary>
/// Base class for Aggregate Roots: the single entry point to an aggregate and
/// the guardian of its invariants. The root records the domain events produced
/// by its behavior so the application layer can dispatch them after persistence.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier of the aggregate root.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>Events recorded since the aggregate was loaded, pending dispatch.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Records a domain event. Only the aggregate itself may call this.</summary>
    protected void RecordEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Clears recorded events, typically once they have been dispatched.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
