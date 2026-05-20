using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

/// <summary>
/// Fact: a discount policy was applied to a cart. Carries the resulting
/// discount value, not the policy itself — only what other contexts need.
/// </summary>
public sealed record DescuentoAplicado(
    CartId CartId,
    decimal DiscountAmount,
    Currency Currency,
    DateTime OccurredOn) : IDomainEvent;
