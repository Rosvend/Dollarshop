using Sales.Domain.Services;

namespace Sales.Application.Abstractions;

/// <summary>
/// Outbound port to <c>finance-service</c> for payment processing. The Application
/// layer declares it; an Infrastructure adapter (Phase 3) implements it by
/// publishing the <c>CheckoutIniciado</c> event through the ACL.
/// <para>
/// The call is one-way: the payment outcome returns asynchronously as the
/// <c>PagoAprobado</c> / <c>PagoRechazado</c> integration events, not as a return
/// value — honoring the event-driven flow of Microservices §3.3.
/// </para>
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>Requests payment for a placed order.</summary>
    Task RequestPaymentAsync(
        OrderPlaced order,
        string paymentMethod,
        CancellationToken cancellationToken = default);
}
