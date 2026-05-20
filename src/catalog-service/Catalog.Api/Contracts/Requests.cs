namespace Catalog.Api.Contracts;

public sealed record PublishProductRequest(
    string Sku,
    string Name,
    decimal ListPrice,
    string Currency,
    int InitialStock);

public sealed record StockReservationRequest(Guid CartId, IReadOnlyList<StockLineRequest> Lines);

public sealed record StockLineRequest(Guid ProductId, int Quantity);
