using MediatR;
using Sales.Application.Exceptions;
using Sales.Application.Mapping;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Commands.ApplyDiscount;

/// <summary>
/// Orchestrates discount application: load the cart, translate the discount DTO
/// into a domain policy, invoke the aggregate, save. The "discount cannot exceed
/// the subtotal" rule is enforced inside <c>ShoppingCart.ApplyDiscount</c>.
/// </summary>
public sealed class ApplyDiscountCommandHandler : IRequestHandler<ApplyDiscountCommand, Unit>
{
    private readonly ICartRepository _carts;

    public ApplyDiscountCommandHandler(ICartRepository carts) => _carts = carts;

    public async Task<Unit> Handle(ApplyDiscountCommand request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(request.CartId), cancellationToken)
            ?? throw new CartNotFoundException(request.CartId);

        var policy = DiscountPolicyMapper.ToDomain(request.Discount);

        cart.ApplyDiscount(policy);

        await _carts.SaveAsync(cart, cancellationToken);

        return Unit.Value;
    }
}
