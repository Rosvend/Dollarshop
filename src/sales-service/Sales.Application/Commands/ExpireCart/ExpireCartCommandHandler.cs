using MediatR;
using Sales.Application.Exceptions;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Commands.ExpireCart;

/// <summary>
/// Orchestrates cart expiry: load the cart, invoke the aggregate, save.
/// Whether the cart may be abandoned is decided inside <c>ShoppingCart.Abandon</c>.
/// </summary>
public sealed class ExpireCartCommandHandler : IRequestHandler<ExpireCartCommand, Unit>
{
    private readonly ICartRepository _carts;

    public ExpireCartCommandHandler(ICartRepository carts) => _carts = carts;

    public async Task<Unit> Handle(ExpireCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(request.CartId), cancellationToken)
            ?? throw new CartNotFoundException(request.CartId);

        cart.Abandon();

        await _carts.SaveAsync(cart, cancellationToken);

        return Unit.Value;
    }
}
