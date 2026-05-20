using Catalog.Application.Dtos;
using Catalog.Application.Exceptions;
using Catalog.Domain.Interfaces;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Queries.GetProduct;

public sealed class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IProductRepository _products;

    public GetProductQueryHandler(IProductRepository products) => _products = products;

    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _products.GetAsync(new ProductId(request.ProductId), cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        return new ProductDto(
            product.Id.Value,
            product.Sku.Value,
            product.Name.Value,
            product.ListPrice.Amount,
            product.ListPrice.Currency.ToString(),
            product.Stock.OnHand);
    }
}
