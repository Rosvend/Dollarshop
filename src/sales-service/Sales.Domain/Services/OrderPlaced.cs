using Sales.Domain.Common;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Services;

/// <summary>
/// The immutable result of transitioning a checked-out <c>ShoppingCart</c> into
/// an order ready to be charged. It is a domain concept of its own — it does
/// not belong to the cart aggregate — and is produced by
/// <see cref="CartTransitionService"/> before being handed to Finance.
/// </summary>
public sealed class OrderPlaced
{
    public OrderPlaced(
        CartId cartId,
        CustomerId customer,
        IReadOnlyList<OrderLine> lines,
        Money total,
        DateTime placedAt)
    {
        if (cartId is null)
        {
            throw new DomainException("An OrderPlaced requires the originating cart id.");
        }

        if (customer is null)
        {
            throw new DomainException("An OrderPlaced requires a customer.");
        }

        if (lines is null || lines.Count == 0)
        {
            throw new DomainException("An OrderPlaced requires at least one line.");
        }

        if (total is null)
        {
            throw new DomainException("An OrderPlaced requires a total.");
        }

        CartId = cartId;
        Customer = customer;
        Lines = lines;
        Total = total;
        PlacedAt = placedAt;
    }

    public CartId CartId { get; }

    public CustomerId Customer { get; }

    public IReadOnlyList<OrderLine> Lines { get; }

    public Money Total { get; }

    public DateTime PlacedAt { get; }
}
