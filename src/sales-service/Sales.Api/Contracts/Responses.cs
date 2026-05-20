namespace Sales.Api.Contracts;

/// <summary>
/// HTTP response contracts — the External layer's own output DTOs, mapped
/// explicitly from the Application result DTOs by <c>ContractMappings</c>.
/// </summary>
public sealed record CreateCartResponse(Guid CartId);

/// <summary>Response body of <c>GET /carts/{cartId}</c>.</summary>
public sealed record CartResponse(
    Guid CartId,
    Guid CustomerId,
    string Status,
    IReadOnlyList<CartItemResponse> Items,
    decimal Subtotal,
    decimal Total,
    string Currency);

/// <summary>A single line of a <see cref="CartResponse"/>.</summary>
public sealed record CartItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineSubtotal);
