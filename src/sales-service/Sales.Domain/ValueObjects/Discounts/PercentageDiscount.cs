using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects.Discounts;

/// <summary>
/// Reduces the subtotal by a percentage. Invariant: <c>Percent ∈ [0, 100]</c>,
/// which guarantees the result is between zero and the original subtotal.
/// </summary>
public sealed class PercentageDiscount : DiscountPolicy
{
    public decimal Percent { get; }

    public PercentageDiscount(decimal percent)
    {
        if (percent < 0m || percent > 100m)
        {
            throw new DomainException("A percentage discount must be within [0, 100].");
        }

        Percent = percent;
    }

    public override Money Apply(Money subtotal) =>
        subtotal.Subtract(subtotal.Percentage(Percent));

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Percent;
    }
}
