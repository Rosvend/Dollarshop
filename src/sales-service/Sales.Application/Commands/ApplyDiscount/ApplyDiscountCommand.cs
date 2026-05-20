using Sales.Application.Dtos;
using Sales.Application.Messaging;

namespace Sales.Application.Commands.ApplyDiscount;

/// <summary>Use case: apply a discount policy to a cart.</summary>
public sealed record ApplyDiscountCommand(Guid CartId, DiscountSpecDto Discount) : ICommand;
