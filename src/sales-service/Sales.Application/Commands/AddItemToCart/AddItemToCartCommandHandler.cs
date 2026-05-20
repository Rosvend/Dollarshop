using MediatR;
using Sales.Application.Exceptions;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Commands.AddItemToCart;

/// <summary>
/// Orchestrates adding an item: load the cart, build the domain Value Objects
/// from the primitive request, invoke the aggregate, save. Product uniqueness
/// and quantity rules are enforced inside <c>ShoppingCart.AddItem</c>.
/// </summary>
public sealed class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, Unit>
{
    private readonly ICartRepository _carts;

    public AddItemToCartCommandHandler(ICartRepository carts) => _carts = carts;

    public async Task<Unit> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(request.CartId), cancellationToken)
            ?? throw new CartNotFoundException(request.CartId);

        var product = new ProductReference(
            new ProductId(request.ProductId),
            request.ProductName,
            new Money(request.UnitPrice, Enum.Parse<Currency>(request.Currency, ignoreCase: true)));

        cart.AddItem(product, new Quantity(request.Quantity));

        await _carts.SaveAsync(cart, cancellationToken);

        return Unit.Value;
    }
}
