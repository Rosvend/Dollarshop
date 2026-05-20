using FluentValidation;

namespace Sales.Application.Commands.CreateCart;

/// <summary>Structural validation only — domain invariants stay in the domain.</summary>
public sealed class CreateCartCommandValidator : AbstractValidator<CreateCartCommand>
{
    public CreateCartCommandValidator()
    {
        RuleFor(c => c.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");
    }
}
