using FluentValidation;

namespace Sales.Application.Commands.ExpireCart;

/// <summary>Structural validation only.</summary>
public sealed class ExpireCartCommandValidator : AbstractValidator<ExpireCartCommand>
{
    public ExpireCartCommandValidator()
    {
        RuleFor(c => c.CartId).NotEmpty();
    }
}
