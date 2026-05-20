using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Events;

/// <summary>Fact: a product was removed from a cart.</summary>
public sealed record CarritoItemRemovido(
    CartId CartId,
    ProductId ProductId,
    DateTime OccurredOn) : IDomainEvent;
