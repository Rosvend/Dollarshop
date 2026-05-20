using System.Text.Json;
using Sales.Application.Abstractions;
using Sales.Domain.Services;
using Sales.Infrastructure.Acl.Contracts;
using Sales.Infrastructure.Outbox;
using Sales.Infrastructure.Persistence;
using Sales.Infrastructure.Serialization;

namespace Sales.Infrastructure.Acl;

/// <summary>
/// Outbound side of the Finance ACL — the Infrastructure implementation of the
/// Application port <see cref="IPaymentGatewayService"/>.
/// <para>
/// Rather than publishing directly (a dual-write risk), it ACL-translates the
/// order into a <see cref="PaymentRequestMessage"/> and stages it into the
/// Transactional Outbox on the <b>same</b> <see cref="SalesDbContext"/>. The use
/// case's <c>CommitAsync</c> persists it atomically with the cart change, and the
/// <c>OutboxRelay</c> delivers it to <c>finance-service</c>.
/// </para>
/// </summary>
public sealed class FinancePaymentGateway : IPaymentGatewayService
{
    private readonly SalesDbContext _dbContext;

    public FinancePaymentGateway(SalesDbContext dbContext) => _dbContext = dbContext;

    public Task RequestPaymentAsync(
        OrderPlaced order,
        string paymentMethod,
        CancellationToken cancellationToken = default)
    {
        var request = FinanceAclMapper.ToPaymentRequest(order, paymentMethod);
        var payload = JsonSerializer.Serialize(request, JsonDefaults.Options);

        _dbContext.EnqueueIntegrationMessage(OutboxMessage.Create(
            messageType: nameof(PaymentRequestMessage),
            routingKey: OutboxRouting.PaymentRequest,
            payload: payload,
            occurredOn: order.PlacedAt));

        return Task.CompletedTask;
    }
}
