using Sales.Domain.Common;
using Sales.Domain.Events;
using Sales.Domain.ValueObjects;
using Sales.Domain.ValueObjects.Discounts;

namespace Sales.Domain.Aggregates;

/// <summary>
/// The Aggregate Root of the Sales core domain. A cart is the only entry point
/// to its items: callers never touch a <see cref="CartItem"/> directly. The
/// root guards every invariant of the aggregate:
/// <list type="number">
///   <item>Product uniqueness — adding an existing product consolidates quantities.</item>
///   <item>Positive quantities — enforced by the <see cref="Quantity"/> Value Object.</item>
///   <item>Non-negative total — <see cref="Money"/> arithmetic rejects negatives.</item>
///   <item>Discount never exceeds the subtotal.</item>
///   <item>Snapshot pricing — a Catalog price change never mutates existing items.</item>
/// </list>
/// State is exposed read-only; it changes only through behavior methods, each of
/// which records a domain event.
/// </summary>
public sealed class ShoppingCart : AggregateRoot<CartId>
{
    private readonly List<CartItem> _items = new();
    private DiscountPolicy? _policy;

    public ShoppingCart(CartId id, CustomerId owner) : base(id)
    {
        Owner = owner ?? throw new DomainException("A ShoppingCart requires an owner.");
        Status = CartStatus.Active;
    }

    /// <summary>The customer who owns this cart.</summary>
    public CustomerId Owner { get; }

    /// <summary>The current lifecycle state of the cart.</summary>
    public CartStatus Status { get; private set; }

    /// <summary>The cart lines, exposed read-only — no direct mutation allowed.</summary>
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    /// <summary>The discount policy applied to the cart, if any.</summary>
    public DiscountPolicy? Discount => _policy;

    /// <summary>
    /// Adds a product to the cart. If the product is already present its
    /// quantity is consolidated, preserving the uniqueness invariant.
    /// </summary>
    public void AddItem(ProductReference product, Quantity quantity)
    {
        EnsureActive();

        if (product is null)
        {
            throw new DomainException("Cannot add a null product to the cart.");
        }

        if (quantity is null)
        {
            throw new DomainException("Cannot add a product without a quantity.");
        }

        var existing = _items.FirstOrDefault(i => i.Product.ProductId == product.ProductId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(CartItemId.New(), product, quantity));
        }

        RecordEvent(new CarritoItemAgregado(Id, product.ProductId, quantity.Value, DateTime.UtcNow));
    }

    /// <summary>Removes a product from the cart. Throws if it is not present.</summary>
    public void RemoveItem(ProductId productId)
    {
        EnsureActive();

        if (productId is null)
        {
            throw new DomainException("Cannot remove an item without a ProductId.");
        }

        var item = _items.FirstOrDefault(i => i.Product.ProductId == productId)
            ?? throw new DomainException("The product is not present in the cart.");

        _items.Remove(item);
        RecordEvent(new CarritoItemRemovido(Id, productId, DateTime.UtcNow));
    }

    /// <summary>
    /// Applies a discount policy. Rejected if the policy would push the total
    /// above the subtotal (it must only ever reduce it).
    /// </summary>
    public void ApplyDiscount(DiscountPolicy policy)
    {
        EnsureActive();

        if (policy is null)
        {
            throw new DomainException("Cannot apply a null discount policy.");
        }

        if (_items.Count == 0)
        {
            throw new DomainException("Cannot apply a discount to an empty cart.");
        }

        var subtotal = CalculateSubtotal();
        var discounted = policy.Apply(subtotal);
        if (discounted.IsGreaterThan(subtotal))
        {
            throw new DomainException("A discount policy cannot increase the cart total.");
        }

        _policy = policy;
        var discountValue = subtotal.Subtract(discounted);
        RecordEvent(new DescuentoAplicado(Id, discountValue.Amount, discountValue.Currency, DateTime.UtcNow));
    }

    /// <summary>The sum of all line subtotals, before any discount.</summary>
    public Money CalculateSubtotal()
    {
        if (_items.Count == 0)
        {
            return Money.Zero();
        }

        return _items
            .Select(item => item.LineSubtotal())
            .Aggregate((running, line) => running.Add(line));
    }

    /// <summary>The subtotal with the discount policy applied, if any.</summary>
    public Money CalculateTotal()
    {
        var subtotal = CalculateSubtotal();
        return _policy is null ? subtotal : _policy.Apply(subtotal);
    }

    /// <summary>
    /// Confirms checkout. Requires an active, non-empty cart with a positive
    /// total. Moves the cart to <see cref="CartStatus.CheckedOut"/>.
    /// </summary>
    public void Checkout()
    {
        EnsureActive();

        if (_items.Count == 0)
        {
            throw new DomainException("Cannot checkout an empty cart.");
        }

        var total = CalculateTotal();
        if (total.IsZero)
        {
            throw new DomainException("Cannot checkout a cart with a zero total.");
        }

        Status = CartStatus.CheckedOut;
        RecordEvent(new CheckoutIniciado(Id, Owner, total.Amount, total.Currency, DateTime.UtcNow));
    }

    /// <summary>
    /// Closes the sale after Finance confirmed the payment. Only valid for a
    /// cart that is currently checked out.
    /// </summary>
    public void ConfirmSale()
    {
        if (Status != CartStatus.CheckedOut)
        {
            throw new DomainException("Only a checked-out cart can be confirmed as a sale.");
        }

        Status = CartStatus.Closed;
        RecordEvent(new VentaCerrada(Id, Owner, DateTime.UtcNow));
    }

    /// <summary>
    /// Reverts a started checkout, typically after a rejected payment. Only
    /// valid for a cart that is currently checked out.
    /// </summary>
    public void RevertCheckout()
    {
        if (Status != CartStatus.CheckedOut)
        {
            throw new DomainException("Only a checked-out cart can be reverted.");
        }

        Status = CartStatus.Reverted;
        RecordEvent(new CheckoutRevertido(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Marks the cart as abandoned (e.g. its session expired). A cart whose
    /// sale is already closed cannot be abandoned.
    /// </summary>
    public void Abandon()
    {
        if (Status == CartStatus.Closed)
        {
            throw new DomainException("A closed sale cannot be abandoned.");
        }

        Status = CartStatus.Abandoned;
        RecordEvent(new CarritoAbandonado(Id, Owner, DateTime.UtcNow));
    }

    private void EnsureActive()
    {
        if (Status != CartStatus.Active)
        {
            throw new DomainException($"Operation not allowed: the cart is {Status}, not Active.");
        }
    }
}
