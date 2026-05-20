using Sales.Domain.Common;

namespace Sales.Domain.ValueObjects.Discounts;

/// <summary>
/// A discount made of several policies applied in sequence (Composite pattern).
/// It treats a group of policies as a single <see cref="DiscountPolicy"/>, so a
/// VIP discount plus a promotional coupon can be applied as one rule.
/// </summary>
public sealed class CompositeDiscount : DiscountPolicy
{
    private readonly List<DiscountPolicy> _policies;

    public CompositeDiscount(IEnumerable<DiscountPolicy> policies)
    {
        if (policies is null)
        {
            throw new DomainException("A composite discount requires a set of policies.");
        }

        _policies = policies.ToList();

        if (_policies.Count == 0)
        {
            throw new DomainException("A composite discount requires at least one policy.");
        }

        if (_policies.Any(p => p is null))
        {
            throw new DomainException("A composite discount cannot contain a null policy.");
        }
    }

    /// <summary>The component policies, in application order.</summary>
    public IReadOnlyList<DiscountPolicy> Policies => _policies.AsReadOnly();

    public override Money Apply(Money subtotal) =>
        _policies.Aggregate(subtotal, (current, policy) => policy.Apply(current));

    protected override IEnumerable<object?> GetEqualityComponents() => _policies;
}
