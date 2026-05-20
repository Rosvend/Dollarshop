using FluentValidation;

namespace Sales.Application.Commands.RemoveItemFromCart;

/// <summary>Structural validation only.</summary>
public sealed class RemoveItemFromCartCommandValidator
    : AbstractValidator<RemoveItemFromCartCommand>
{
    public RemoveItemFromCartCommandValidator()
    {
        RuleFor(c => c.CartId).NotEmpty();
        RuleFor(c => c.ProductId).NotEmpty();
    }
}
