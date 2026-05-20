using Sales.Domain.Common;
using Sales.Domain.Events;

namespace Sales.Infrastructure.Outbox;

/// <summary>
/// Maps a sales domain event to the topic-exchange routing key the Outbox relay
/// publishes it with (Microservices §3.2). Sales facts travel under the
/// <c>sales.*</c> namespace; the ACL payment request travels under
/// <c>finance.*</c> toward <c>finance-service</c>.
/// </summary>
internal static class OutboxRouting
{
    /// <summary>Routing key carrying the ACL-translated payment request to Finance.</summary>
    public const string PaymentRequest = "finance.payment.request";

    private static readonly IReadOnlyDictionary<Type, string> EventRoutingKeys = new Dictionary<Type, string>
    {
        [typeof(CarritoItemAgregado)] = "sales.carrito.item-agregado",
        [typeof(CarritoItemRemovido)] = "sales.carrito.item-removido",
        [typeof(DescuentoAplicado)] = "sales.carrito.descuento-aplicado",
        [typeof(CheckoutIniciado)] = "sales.checkout.iniciado",
        [typeof(VentaCerrada)] = "sales.venta.cerrada",
        [typeof(CheckoutRevertido)] = "sales.checkout.revertido",
        [typeof(CarritoAbandonado)] = "sales.carrito.abandonado",
    };

    public static string ForEvent(IDomainEvent domainEvent) =>
        EventRoutingKeys.TryGetValue(domainEvent.GetType(), out var routingKey)
            ? routingKey
            : $"sales.event.{domainEvent.GetType().Name.ToLowerInvariant()}";
}
