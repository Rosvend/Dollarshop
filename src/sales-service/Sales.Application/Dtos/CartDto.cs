namespace Sales.Application.Dtos;

/// <summary>Read model of a shopping cart, returned by <c>GetCartQuery</c>.</summary>
public sealed record CartDto(
    Guid CartId,
    Guid CustomerId,
    string Status,
    IReadOnlyList<CartItemDto> Items,
    decimal Subtotal,
    decimal Total,
    string Currency);
