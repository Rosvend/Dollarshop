using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

/// <summary>
/// Fact: a started checkout was reverted, typically because Finance rejected
/// the payment. It triggers the compensating steps of the checkout Saga.
/// </summary>
public sealed record CheckoutRevertido(
    CartId CartId,
    DateTime OccurredOn) : IDomainEvent;
