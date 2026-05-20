namespace Sales.Infrastructure.Acl.Contracts;

/// <summary>
/// Outbound contract toward <c>finance-service</c>. This is <b>Finance's</b>
/// vocabulary ("transaction", "SKU"), deliberately different from the Sales
/// domain model. It exists only inside the ACL boundary (Microservices §5) — no
/// type below ever crosses into the Sales Domain or Application layers.
/// </summary>
public sealed record PaymentRequestMessage(
    Guid TransactionReference,
    Guid CustomerReference,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethod,
    IReadOnlyList<PaymentLineItem> Items,
    DateTimeOffset RequestedAt);

/// <summary>A single line of a <see cref="PaymentRequestMessage"/>.</summary>
public sealed record PaymentLineItem(
    Guid Sku,
    string Description,
    int Units,
    decimal UnitPrice);
