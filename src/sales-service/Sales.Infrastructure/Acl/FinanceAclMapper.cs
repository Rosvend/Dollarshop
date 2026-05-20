using MediatR;
using Sales.Application.IntegrationEvents;
using Sales.Domain.Services;
using Sales.Infrastructure.Acl.Contracts;

namespace Sales.Infrastructure.Acl;

/// <summary>
/// The Anti-Corruption Layer between Ventas and Finanzas (Microservices §5) — a
/// bidirectional translator that keeps the two vocabularies apart:
/// <list type="bullet">
///   <item><b>Outbound</b> — <see cref="ToPaymentRequest"/> turns the Sales domain
///   object <see cref="OrderPlaced"/> into Finance's <see cref="PaymentRequestMessage"/>.</item>
///   <item><b>Inbound</b> — <see cref="ToNotification"/> turns Finance's
///   <see cref="PaymentResultMessage"/> back into a Sales integration event
///   (<see cref="PagoAprobado"/> / <see cref="PagoRechazado"/>).</item>
/// </list>
/// No <c>finance-service</c> type is ever exposed beyond this class.
/// </summary>
internal static class FinanceAclMapper
{
    /// <summary>Outbound translation: Sales domain order → Finance payment request.</summary>
    public static PaymentRequestMessage ToPaymentRequest(OrderPlaced order, string paymentMethod) =>
        new(
            TransactionReference: order.CartId.Value,
            CustomerReference: order.Customer.Value,
            Amount: order.Total.Amount,
            CurrencyCode: order.Total.Currency.ToString(),
            PaymentMethod: paymentMethod,
            Items: order.Lines
                .Select(line => new PaymentLineItem(
                    Sku: line.Product.ProductId.Value,
                    Description: line.Product.SnapshotName,
                    Units: line.Quantity.Value,
                    UnitPrice: line.Product.SnapshotPrice.Amount))
                .ToList(),
            RequestedAt: DateTimeOffset.UtcNow);

    /// <summary>Inbound translation: Finance payment result → Sales integration event.</summary>
    public static INotification ToNotification(PaymentResultMessage result) =>
        IsApproved(result.Outcome)
            ? new PagoAprobado(result.TransactionReference, result.ProcessedAt.UtcDateTime)
            : new PagoRechazado(
                result.TransactionReference,
                result.DeclineReason ?? "Payment declined by finance-service.",
                result.ProcessedAt.UtcDateTime);

    private static bool IsApproved(string outcome) =>
        outcome.Equals("AUTHORIZED", StringComparison.OrdinalIgnoreCase)
        || outcome.Equals("APPROVED", StringComparison.OrdinalIgnoreCase);
}
