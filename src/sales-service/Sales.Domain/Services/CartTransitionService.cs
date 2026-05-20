using Sales.Domain.Aggregates;
using Sales.Domain.Common;

namespace Sales.Domain.Services;

/// <summary>
/// Pure domain service. Materializes the <c>ShoppingCart → OrderPlaced</c>
/// transition: it produces a new immutable object that the cart does not own,
/// which is why this logic lives in a service and not in the aggregate.
/// </summary>
public sealed class CartTransitionService
{
    /// <summary>
    /// Builds an <see cref="OrderPlaced"/> from a checked-out cart. The cart
    /// must already be in <see cref="CartStatus.CheckedOut"/>.
    /// </summary>
    public OrderPlaced PlaceOrder(ShoppingCart cart)
    {
        if (cart is null)
        {
            throw new DomainException("Cannot place an order from a null cart.");
        }

        if (cart.Status != CartStatus.CheckedOut)
        {
            throw new DomainException("An order can only be placed from a checked-out cart.");
        }

        var lines = cart.Items
            .Select(item => new OrderLine(item.Product, item.Quantity, item.LineSubtotal()))
            .ToList();

        return new OrderPlaced(cart.Id, cart.Owner, lines, cart.CalculateTotal(), DateTime.UtcNow);
    }
}
