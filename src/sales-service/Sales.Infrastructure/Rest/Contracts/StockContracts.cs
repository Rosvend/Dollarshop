namespace Sales.Infrastructure.Rest.Contracts;

/// <summary>HTTP request body sent to <c>catalog-service</c> to reserve or release stock.</summary>
public sealed record StockReservationRequest(Guid CartId, IReadOnlyList<StockLineDto> Lines);

/// <summary>A single product line of a <see cref="StockReservationRequest"/>.</summary>
public sealed record StockLineDto(Guid ProductId, int Quantity);
