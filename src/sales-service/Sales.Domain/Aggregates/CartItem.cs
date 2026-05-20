using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Aggregates;

/// <summary>
/// A single line of a cart: a product snapshot plus a quantity. It is an Entity
/// — its quantity can change without replacing the whole line — but its
/// identity is meaningful only inside the owning <see cref="ShoppingCart"/>.
/// For that reason both its constructor and its mutators are <c>internal</c>:
/// only the aggregate root can create or change a <see cref="CartItem"/>.
/// </summary>
public sealed class CartItem : Entity<CartItemId>
{
    public ProductReference Product { get; }

    public Quantity Quantity { get; private set; }

    internal CartItem(CartItemId id, ProductReference product, Quantity quantity)
        : base(id)
    {
        Product = product ?? throw new DomainException("A CartItem requires a product reference.");
        Quantity = quantity ?? throw new DomainException("A CartItem requires a quantity.");
    }

    /// <summary>The line total: snapshot price multiplied by the quantity.</summary>
    public Money LineSubtotal() => Product.SnapshotPrice.Multiply(Quantity);

    /// <summary>Consolidates an additional quantity into this line.</summary>
    internal void IncreaseQuantity(Quantity by) => Quantity = Quantity.Increase(by);
}
