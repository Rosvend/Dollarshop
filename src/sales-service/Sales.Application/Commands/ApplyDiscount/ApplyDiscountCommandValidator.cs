using FluentValidation;

namespace Sales.Application.Commands.ApplyDiscount;

/// <summary>Structural validation only — the discount's numeric ranges are domain rules.</summary>
public sealed class ApplyDiscountCommandValidator : AbstractValidator<ApplyDiscountCommand>
{
    public ApplyDiscountCommandValidator()
    {
        RuleFor(c => c.CartId).NotEmpty();
        RuleFor(c => c.Discount).NotNull().WithMessage("A discount specification is required.");
    }
}
