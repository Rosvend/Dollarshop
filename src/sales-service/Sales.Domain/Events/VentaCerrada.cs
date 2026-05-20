using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

/// <summary>
/// Fact: the sale was closed after Finance confirmed the payment. It enables
/// invoice issuance in the Finance context.
/// </summary>
public sealed record VentaCerrada(
    CartId CartId,
    CustomerId CustomerId,
    DateTime OccurredOn) : IDomainEvent;
