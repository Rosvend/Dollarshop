using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects.Discounts;

/// <summary>
/// A discount rule: it knows how to turn a subtotal into a discounted amount.
/// It is a Value Object — defined by its value, not by identity (two 10%
/// discounts are the same discount). Concrete variants are polymorphic:
/// <see cref="PercentageDiscount"/>, <see cref="FixedAmountDiscount"/> and
/// <see cref="CompositeDiscount"/>.
/// </summary>
public abstract class DiscountPolicy : ValueObject
{
    /// <summary>
    /// Applies the policy to <paramref name="subtotal"/> and returns the
    /// resulting amount. Implementations must never return a negative Money.
    /// </summary>
    public abstract Money Apply(Money subtotal);
}
