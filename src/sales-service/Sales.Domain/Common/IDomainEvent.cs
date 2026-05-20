namespace Sales.Domain.Common;

/// <summary>
/// Marker for a fact that already happened in the domain. Domain events are
/// immutable and named in the past tense (the business' ubiquitous language).
/// They are recorded by an <see cref="AggregateRoot{TId}"/> and dispatched later.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Instant (UTC) at which the fact occurred.</summary>
    DateTime OccurredOn { get; }
}
