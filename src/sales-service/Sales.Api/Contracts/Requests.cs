using Sales.Application.Dtos;

namespace Sales.Api.Contracts;

/// <summary>
/// HTTP request contracts — the External layer's own input DTOs. They are mapped
/// explicitly to Application Commands by <c>ContractMappings</c>; an Application
/// Command is never bound directly from the request body. Identifiers that belong
/// in the URL (e.g. <c>cartId</c>) are taken from the route, not the body.
/// </summary>
public sealed record CreateCartRequest(Guid CustomerId);

/// <summary>Body of <c>POST /carts/{cartId}/items</c>.</summary>
public sealed record AddItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity);

/// <summary>Body of <c>POST /carts/{cartId}/checkout</c>.</summary>
public sealed record CheckoutRequest(string PaymentMethod);

/// <summary>Body of <c>POST /carts/{cartId}/discounts</c>.</summary>
public sealed record ApplyDiscountRequest(DiscountRequest Discount);

/// <summary>
/// Transport shape of a discount policy. Discriminated by <see cref="Kind"/> and
/// recursive (a <c>Composite</c> carries nested <see cref="Components"/>) — it
/// mirrors the domain discount hierarchy and is mapped to the Application
/// <see cref="DiscountSpecDto"/>.
/// </summary>
public sealed record DiscountRequest(
    DiscountKind Kind,
    decimal? Percentage = null,
    MoneyRequest? Amount = null,
    IReadOnlyList<DiscountRequest>? Components = null);

/// <summary>Transport shape of a monetary amount inside a <see cref="DiscountRequest"/>.</summary>
public sealed record MoneyRequest(decimal Amount, string Currency);
