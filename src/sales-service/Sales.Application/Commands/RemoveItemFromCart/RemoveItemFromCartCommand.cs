using Sales.Application.Messaging;

namespace Sales.Application.Commands.RemoveItemFromCart;

/// <summary>Use case: remove a product from a cart.</summary>
public sealed record RemoveItemFromCartCommand(Guid CartId, Guid ProductId) : ICommand;
