using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

/// <summary>
/// Fact: a product line was added to (or consolidated into) a cart.
/// Named in the past tense, in the business' ubiquitous language.
/// </summary>
public sealed record CarritoItemAgregado(
    CartId CartId,
    ProductId ProductId,
    int Quantity,
    DateTime OccurredOn) : IDomainEvent;
