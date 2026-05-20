using Sales.Application.Messaging;

namespace Sales.Application.Commands.CreateCart;

/// <summary>Use case: open a new, empty shopping cart for a customer.</summary>
/// <returns>The identifier of the newly created cart.</returns>
public sealed record CreateCartCommand(Guid CustomerId) : ICommand<Guid>;
