namespace Sales.Application.Dtos;

/// <summary>Discriminator for the polymorphic <see cref="DiscountSpecDto"/>.</summary>
public enum DiscountKind
{
    Percentage,
    FixedAmount,
    Composite
}

/// <summary>
/// Transport shape of a discount policy. It is a discriminated structure: the
/// fields that matter depend on <see cref="Kind"/>. It is mapped to the domain
/// <c>DiscountPolicy</c> VO hierarchy by <c>DiscountPolicyMapper</c>.
/// </summary>
public sealed record DiscountSpecDto(
    DiscountKind Kind,
    decimal? Percentage = null,
    MoneyDto? Amount = null,
    IReadOnlyList<DiscountSpecDto>? Components = null);
