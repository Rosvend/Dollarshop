using Sales.Application.Messaging;

namespace Sales.Application.Commands.AddItemToCart;

/// <summary>
/// Use case: add a product to a cart. The command is itself the Application
/// input contract — a plain record of primitives, no domain objects.
/// </summary>
public sealed record AddItemToCartCommand(
    Guid CartId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity) : ICommand;
