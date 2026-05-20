using Sales.Application.Dtos;
using Sales.Domain.ValueObjects.Discounts;

namespace Sales.Infrastructure.Mapping;

/// <summary>
/// Maps the domain <see cref="DiscountPolicy"/> Value Object hierarchy back into
/// the transport <see cref="DiscountSpecDto"/>. It is the exact inverse of
/// <c>Sales.Application.Mapping.DiscountPolicyMapper.ToDomain</c> and exists so the
/// polymorphic discount can be stored as a single JSON column without the Domain
/// gaining any persistence concern.
/// </summary>
internal static class DiscountPolicyDtoMapper
{
    public static DiscountSpecDto ToDto(DiscountPolicy policy) => policy switch
    {
        PercentageDiscount percentage => new DiscountSpecDto(
            DiscountKind.Percentage,
            Percentage: percentage.Percent),

        FixedAmountDiscount fixedAmount => new DiscountSpecDto(
            DiscountKind.FixedAmount,
            Amount: new MoneyDto(fixedAmount.Amount.Amount, fixedAmount.Amount.Currency.ToString())),

        CompositeDiscount composite => new DiscountSpecDto(
            DiscountKind.Composite,
            Components: composite.Policies.Select(ToDto).ToList()),

        _ => throw new ArgumentOutOfRangeException(
            nameof(policy), policy.GetType().Name, "Unknown discount policy type.")
    };
}
