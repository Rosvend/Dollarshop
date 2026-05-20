using FluentValidation;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Commands.AddItemToCart;

/// <summary>
/// Structural validation of the request shape only. Business invariants
/// (<c>Quantity &gt;= 1</c>, <c>UnitPrice &gt;= 0</c>) are NOT checked here —
/// they belong to the domain, which throws <c>DomainException</c>.
/// </summary>
public sealed class AddItemToCartCommandValidator : AbstractValidator<AddItemToCartCommand>
{
    public AddItemToCartCommandValidator()
    {
        RuleFor(c => c.CartId).NotEmpty();
        RuleFor(c => c.ProductId).NotEmpty();
        RuleFor(c => c.ProductName).NotEmpty();
        RuleFor(c => c.Currency)
            .NotEmpty()
            .Must(BeAKnownCurrency)
            .WithMessage("Currency must be one of: USD, COP, EUR.");
    }

    private static bool BeAKnownCurrency(string currency) =>
        Enum.TryParse<Currency>(currency, ignoreCase: true, out _);
}
