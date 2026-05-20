using Sales.Application.Dtos;
using Sales.Domain.ValueObjects;
using Sales.Domain.ValueObjects.Discounts;

namespace Sales.Application.Mapping;

/// <summary>
/// Translates a <see cref="DiscountSpecDto"/> (transport shape) into the domain
/// <see cref="DiscountPolicy"/> VO hierarchy. This is pure DTO-to-VO mapping:
/// the policies' invariants (percentage range, non-negative amount) are enforced
/// by the VO constructors, not re-checked here.
/// </summary>
public static class DiscountPolicyMapper
{
    public static DiscountPolicy ToDomain(DiscountSpecDto spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        return spec.Kind switch
        {
            DiscountKind.Percentage => new PercentageDiscount(
                spec.Percentage ?? throw MissingField("Percentage", spec.Kind)),

            DiscountKind.FixedAmount => new FixedAmountDiscount(
                ToMoney(spec.Amount ?? throw MissingField("Amount", spec.Kind))),

            DiscountKind.Composite => new CompositeDiscount(
                (spec.Components ?? throw MissingField("Components", spec.Kind))
                    .Select(ToDomain)),

            _ => throw new ArgumentOutOfRangeException(
                nameof(spec), spec.Kind, "Unknown discount kind.")
        };
    }

    private static Money ToMoney(MoneyDto dto) =>
        new(dto.Amount, Enum.Parse<Currency>(dto.Currency, ignoreCase: true));

    private static ArgumentException MissingField(string field, DiscountKind kind) =>
        new($"A '{kind}' discount requires the '{field}' field.");
}
