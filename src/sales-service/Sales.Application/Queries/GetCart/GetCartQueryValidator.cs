using FluentValidation;

namespace Sales.Application.Queries.GetCart;

/// <summary>Structural validation only.</summary>
public sealed class GetCartQueryValidator : AbstractValidator<GetCartQuery>
{
    public GetCartQueryValidator()
    {
        RuleFor(q => q.CartId).NotEmpty();
    }
}
