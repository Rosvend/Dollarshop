using Catalog.Domain.Aggregates;
using Catalog.Domain.Interfaces;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Commands.PublishProduct;

public sealed class PublishProductCommandHandler : IRequestHandler<PublishProductCommand, Guid>
{
    private readonly IProductRepository _products;

    public PublishProductCommandHandler(IProductRepository products) => _products = products;

    public async Task<Guid> Handle(PublishProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(
            ProductId.New(),
            new Sku(request.Sku),
            new ProductName(request.Name),
            new Money(request.ListPrice, Enum.Parse<Currency>(request.Currency, ignoreCase: true)),
            new StockLevel(request.InitialStock));

        await _products.SaveAsync(product, cancellationToken);

        return product.Id.Value;
    }
}
