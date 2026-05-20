namespace Sales.Application.Dtos;

/// <summary>Read model of a single cart line, returned by <c>GetCartQuery</c>.</summary>
public sealed record CartItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineSubtotal);
