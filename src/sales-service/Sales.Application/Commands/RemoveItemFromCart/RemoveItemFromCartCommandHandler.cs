using MediatR;
using Sales.Application.Exceptions;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Commands.RemoveItemFromCart;

/// <summary>
/// Orchestrates item removal: load the cart, invoke the aggregate, save.
/// The "item must exist" rule is enforced inside <c>ShoppingCart.RemoveItem</c>.
/// </summary>
public sealed class RemoveItemFromCartCommandHandler
    : IRequestHandler<RemoveItemFromCartCommand, Unit>
{
    private readonly ICartRepository _carts;

    public RemoveItemFromCartCommandHandler(ICartRepository carts) => _carts = carts;

    public async Task<Unit> Handle(RemoveItemFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(request.CartId), cancellationToken)
            ?? throw new CartNotFoundException(request.CartId);

        cart.RemoveItem(new ProductId(request.ProductId));

        await _carts.SaveAsync(cart, cancellationToken);

        return Unit.Value;
    }
}
