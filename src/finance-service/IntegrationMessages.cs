namespace Finance.Api;

/// <summary>
/// Inbound contract from <c>sales-service</c> via the ACL (Microservices §5).
/// Field shape and order match sales' <c>Sales.Infrastructure.Acl.Contracts.PaymentRequestMessage</c>
/// — kept as a separate copy so the two services share no assembly.
/// </summary>
internal sealed record PaymentRequestMessage(
    Guid TransactionReference,
    Guid CustomerReference,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethod,
    IReadOnlyList<PaymentLineItem> Items,
    DateTimeOffset RequestedAt);

/// <summary>A single line of a <see cref="PaymentRequestMessage"/>.</summary>
internal sealed record PaymentLineItem(
    Guid Sku,
    string Description,
    int Units,
    decimal UnitPrice);

/// <summary>
/// Outbound result for <c>sales-service</c>. <see cref="Outcome"/> uses the
/// provider's own status string — the sales ACL accepts <c>"AUTHORIZED"</c>
/// (or <c>"APPROVED"</c>) as the approved path.
/// </summary>
internal sealed record PaymentResultMessage(
    Guid TransactionReference,
    string Outcome,
    string? DeclineReason,
    DateTimeOffset ProcessedAt);
