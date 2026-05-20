using Catalog.Api.Contracts;
using Catalog.Application.Commands.PublishProduct;
using Catalog.Application.Commands.ReleaseStock;
using Catalog.Application.Commands.ReserveStock;
using Catalog.Application.Dtos;
using Catalog.Application.Queries.GetProduct;
using Catalog.Application.Queries.ListProducts;

namespace Catalog.Api.Mapping;

internal static class ContractMappings
{
    public static PublishProductCommand ToCommand(this PublishProductRequest request) =>
        new(request.Sku, request.Name, request.ListPrice, request.Currency, request.InitialStock);

    public static ReserveStockCommand ToReserveCommand(this StockReservationRequest request) =>
        new(request.CartId, request.Lines.Select(line => line.ToDto()).ToList());

    public static ReleaseStockCommand ToReleaseCommand(this StockReservationRequest request) =>
        new(request.CartId, request.Lines.Select(line => line.ToDto()).ToList());

    public static ProductResponse ToResponse(this ProductDto dto) =>
        new(dto.Id, dto.Sku, dto.Name, dto.ListPrice, dto.Currency, dto.StockOnHand);

    public static ProductResponse ToResponse(this ProductSummaryDto dto) =>
        new(dto.Id, dto.Sku, dto.Name, dto.ListPrice, dto.Currency, dto.StockOnHand);

    public static GetProductQuery ToQuery(Guid productId) => new(productId);

    public static ListProductsQuery ToListQuery() => new();

    private static StockLineDto ToDto(this StockLineRequest line) =>
        new(line.ProductId, line.Quantity);
}
