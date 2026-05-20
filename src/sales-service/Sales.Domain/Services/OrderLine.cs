using Sales.Domain.ValueObjects;

namespace Sales.Domain.Services;

/// <summary>
/// A frozen line of a placed order: the product snapshot, the quantity and the
/// line subtotal captured at the moment of the checkout transition. Immutable.
/// </summary>
public sealed record OrderLine(ProductReference Product, Quantity Quantity, Money LineSubtotal);
