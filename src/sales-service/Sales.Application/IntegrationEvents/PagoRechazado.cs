using MediatR;

namespace Sales.Application.IntegrationEvents;

/// <summary>
/// Incoming integration event: <c>finance-service</c> rejected the payment.
/// Triggers the compensating branch of the checkout Saga.
/// </summary>
public sealed record PagoRechazado(Guid CartId, string Reason, DateTime OccurredOn) : INotification;
