using MediatR;

namespace Sales.Application.IntegrationEvents;

/// <summary>
/// Incoming integration event: <c>finance-service</c> approved the payment.
/// The Infrastructure ACL (Phase 3) translates finance's <c>PaymentResult</c>
/// into this contract before publishing it through the mediator.
/// </summary>
public sealed record PagoAprobado(Guid CartId, DateTime OccurredOn) : INotification;
