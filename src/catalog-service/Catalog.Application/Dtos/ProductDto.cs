namespace Catalog.Application.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    decimal ListPrice,
    string Currency,
    int StockOnHand);

public sealed record ProductSummaryDto(
    Guid Id,
    string Sku,
    string Name,
    decimal ListPrice,
    string Currency,
    int StockOnHand);
