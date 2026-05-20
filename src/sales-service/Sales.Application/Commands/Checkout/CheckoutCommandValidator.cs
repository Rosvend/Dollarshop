using FluentValidation;

namespace Sales.Application.Commands.Checkout;

/// <summary>Structural validation only — checkout invariants live in the domain.</summary>
public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(c => c.CartId).NotEmpty();
        RuleFor(c => c.PaymentMethod)
            .NotEmpty().WithMessage("A payment method is required.");
    }
}
