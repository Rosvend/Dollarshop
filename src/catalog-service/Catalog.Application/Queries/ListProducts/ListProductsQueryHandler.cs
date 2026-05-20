using Catalog.Application.Dtos;
using Catalog.Domain.Interfaces;
using MediatR;

namespace Catalog.Application.Queries.ListProducts;

public sealed class ListProductsQueryHandler
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductSummaryDto>>
{
    private readonly IProductRepository _products;

    public ListProductsQueryHandler(IProductRepository products) => _products = products;

    public async Task<IReadOnlyList<ProductSummaryDto>> Handle(
        ListProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _products.ListAsync(cancellationToken);

        return products
            .Select(product => new ProductSummaryDto(
                product.Id.Value,
                product.Sku.Value,
                product.Name.Value,
                product.ListPrice.Amount,
                product.ListPrice.Currency.ToString(),
                product.Stock.OnHand))
            .ToList();
    }
}
