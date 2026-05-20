using Sales.Api.Contracts;
using Sales.Application.Commands.AddItemToCart;
using Sales.Application.Commands.ApplyDiscount;
using Sales.Application.Commands.Checkout;
using Sales.Application.Commands.CreateCart;
using Sales.Application.Commands.RemoveItemFromCart;
using Sales.Application.Dtos;

namespace Sales.Api.Mapping;

/// <summary>
/// Explicit translation between the External layer's HTTP contracts and the
/// Application layer's Commands / result DTOs (rubric: "Mapeo explícito
/// hacia/desde el modelo"). Keeping this mapping in one place keeps the
/// controller thin — it only calls these methods and dispatches.
/// </summary>
internal static class ContractMappings
{
    // ---- HTTP request -> Application command -------------------------------

    public static CreateCartCommand ToCommand(this CreateCartRequest request) =>
        new(request.CustomerId);

    public static AddItemToCartCommand ToCommand(this AddItemRequest request, Guid cartId) =>
        new(
            CartId: cartId,
            ProductId: request.ProductId,
            ProductName: request.ProductName,
            UnitPrice: request.UnitPrice,
            Currency: request.Currency,
            Quantity: request.Quantity);

    public static RemoveItemFromCartCommand ToRemoveItemCommand(Guid cartId, Guid productId) =>
        new(cartId, productId);

    public static ApplyDiscountCommand ToCommand(this ApplyDiscountRequest request, Guid cartId) =>
        new(cartId, ToSpec(request.Discount));

    public static CheckoutCommand ToCommand(this CheckoutRequest request, Guid cartId) =>
        new(cartId, request.PaymentMethod);

    /// <summary>Recursively maps the HTTP discount contract to the Application discount DTO.</summary>
    private static DiscountSpecDto ToSpec(DiscountRequest discount) =>
        new(
            Kind: discount.Kind,
            Percentage: discount.Percentage,
            Amount: discount.Amount is null
                ? null
                : new MoneyDto(discount.Amount.Amount, discount.Amount.Currency),
            Components: discount.Components?.Select(ToSpec).ToList());

    // ---- Application result DTO -> HTTP response ---------------------------

    public static CartResponse ToResponse(this CartDto cart) =>
        new(
            CartId: cart.CartId,
            CustomerId: cart.CustomerId,
            Status: cart.Status,
            Items: cart.Items.Select(ToItemResponse).ToList(),
            Subtotal: cart.Subtotal,
            Total: cart.Total,
            Currency: cart.Currency);

    private static CartItemResponse ToItemResponse(CartItemDto item) =>
        new(
            ProductId: item.ProductId,
            ProductName: item.ProductName,
            UnitPrice: item.UnitPrice,
            Currency: item.Currency,
            Quantity: item.Quantity,
            LineSubtotal: item.LineSubtotal);
}
