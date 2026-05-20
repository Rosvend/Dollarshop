using Sales.Domain.Aggregates;
using Sales.Domain.Common;
using Sales.Domain.ValueObjects;
using Sales.Domain.ValueObjects.Discounts;

namespace Sales.Domain.Services;

/// <summary>
/// Pure domain service. Composes several heterogeneous discount policies (for
/// example a VIP discount plus a promotional coupon) into a single
/// <see cref="CompositeDiscount"/>. This cross-policy logic belongs to no
/// single <see cref="DiscountPolicy"/>, hence a domain service.
/// </summary>
public sealed class CartPricingService
{
    /// <summary>Combines the given policies into one composite policy.</summary>
    public CompositeDiscount Combine(params DiscountPolicy[] policies)
    {
        if (policies is null || policies.Length == 0)
        {
            throw new DomainException("At least one discount policy is required to combine.");
        }

        return new CompositeDiscount(policies);
    }

    /// <summary>
    /// Prices a cart with a combined policy, returning the discounted total
    /// without mutating the cart.
    /// </summary>
    public Money PriceWith(ShoppingCart cart, CompositeDiscount combined)
    {
        if (cart is null)
        {
            throw new DomainException("Cannot price a null cart.");
        }

        if (combined is null)
        {
            throw new DomainException("Cannot price a cart without a combined policy.");
        }

        return combined.Apply(cart.CalculateSubtotal());
    }
}
