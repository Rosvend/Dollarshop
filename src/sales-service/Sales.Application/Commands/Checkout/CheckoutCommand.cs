using Sales.Application.Messaging;

namespace Sales.Application.Commands.Checkout;

/// <summary>Use case: start the checkout Saga for a cart.</summary>
public sealed record CheckoutCommand(Guid CartId, string PaymentMethod) : ICommand;
