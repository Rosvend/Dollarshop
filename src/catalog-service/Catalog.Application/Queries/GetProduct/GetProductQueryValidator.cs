using FluentValidation;

namespace Catalog.Application.Queries.GetProduct;

public sealed class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
    }
}
