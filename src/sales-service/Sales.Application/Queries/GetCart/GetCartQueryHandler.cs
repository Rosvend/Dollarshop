using MediatR;
using Sales.Application.Dtos;
using Sales.Application.Exceptions;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Queries.GetCart;

/// <summary>
/// Read-only handler: loads the cart aggregate and projects it to a flat
/// <see cref="CartDto"/>. It never mutates state and is not transactional.
/// </summary>
public sealed class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly ICartRepository _carts;

    public GetCartQueryHandler(ICartRepository carts) => _carts = carts;

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(request.CartId), cancellationToken)
            ?? throw new CartNotFoundException(request.CartId);

        var items = cart.Items
            .Select(item => new CartItemDto(
                item.Product.ProductId.Value,
                item.Product.SnapshotName,
                item.Product.SnapshotPrice.Amount,
                item.Product.SnapshotPrice.Currency.ToString(),
                item.Quantity.Value,
                item.LineSubtotal().Amount))
            .ToList();

        var subtotal = cart.CalculateSubtotal();
        var total = cart.CalculateTotal();

        return new CartDto(
            cart.Id.Value,
            cart.Owner.Value,
            cart.Status.ToString(),
            items,
            subtotal.Amount,
            total.Amount,
            total.Currency.ToString());
    }
}
