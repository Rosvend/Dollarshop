using MediatR;
using Sales.Domain.Aggregates;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Commands.CreateCart;

/// <summary>
/// Orchestrates cart creation: build the aggregate, persist it. No business
/// logic — the <c>ShoppingCart</c> constructor enforces its own invariants.
/// </summary>
public sealed class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, Guid>
{
    private readonly ICartRepository _carts;

    public CreateCartCommandHandler(ICartRepository carts) => _carts = carts;

    public async Task<Guid> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = new ShoppingCart(CartId.New(), new CustomerId(request.CustomerId));

        await _carts.SaveAsync(cart, cancellationToken);

        return cart.Id.Value;
    }
}
