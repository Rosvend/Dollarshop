using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

/// <summary>
/// Fact: a cart was abandoned without completing checkout (e.g. its session
/// expired). Useful for downstream recovery campaigns.
/// </summary>
public sealed record CarritoAbandonado(
    CartId CartId,
    CustomerId CustomerId,
    DateTime OccurredOn) : IDomainEvent;
