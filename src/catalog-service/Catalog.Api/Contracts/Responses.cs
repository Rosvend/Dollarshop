namespace Catalog.Api.Contracts;

public sealed record PublishProductResponse(Guid ProductId);

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    decimal ListPrice,
    string Currency,
    int StockOnHand);

public sealed record ProductListResponse(IReadOnlyList<ProductResponse> Products);
