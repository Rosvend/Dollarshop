namespace Sales.Infrastructure.Acl.Contracts;

/// <summary>
/// Inbound contract from <c>finance-service</c>. Again expressed in Finance's
/// vocabulary — note <see cref="Outcome"/> is the provider's own status string
/// ("AUTHORIZED" / "DECLINED"), which the ACL translates so it never reaches the
/// Sales model.
/// </summary>
public sealed record PaymentResultMessage(
    Guid TransactionReference,
    string Outcome,
    string? DeclineReason,
    DateTimeOffset ProcessedAt);
