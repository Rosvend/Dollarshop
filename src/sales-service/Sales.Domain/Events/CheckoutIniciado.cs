using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

/// <summary>
/// Fact: the customer confirmed checkout. This event crosses to the Finance
/// context (via the ACL) to request payment processing.
/// </summary>
public sealed record CheckoutIniciado(
    CartId CartId,
    CustomerId CustomerId,
    decimal Total,
    Currency Currency,
    DateTime OccurredOn) : IDomainEvent;
