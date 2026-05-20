using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects.Discounts;

/// <summary>
/// Reduces the subtotal by a fixed monetary amount. If the discount exceeds the
/// subtotal it clamps to zero — it never drives the total below zero.
/// </summary>
public sealed class FixedAmountDiscount : DiscountPolicy
{
    public Money Amount { get; }

    public FixedAmountDiscount(Money amount)
    {
        if (amount is null)
        {
            throw new DomainException("A fixed amount discount requires an amount.");
        }

        Amount = amount;
    }

    public override Money Apply(Money subtotal) =>
        Amount.IsGreaterThan(subtotal)
            ? Money.Zero(subtotal.Currency)
            : subtotal.Subtract(Amount);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }
}
