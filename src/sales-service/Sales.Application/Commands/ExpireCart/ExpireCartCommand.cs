using Sales.Application.Messaging;

namespace Sales.Application.Commands.ExpireCart;

/// <summary>
/// Use case: expire a cart whose session ended without checkout (DDD trigger #7).
/// </summary>
public sealed record ExpireCartCommand(Guid CartId) : ICommand;
