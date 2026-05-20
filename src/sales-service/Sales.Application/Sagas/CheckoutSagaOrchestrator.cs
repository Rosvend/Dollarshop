using Sales.Application.Abstractions;
using Sales.Application.Exceptions;
using Sales.Domain.Aggregates;
using Sales.Domain.Interfaces;
using Sales.Domain.Services;
using Sales.Domain.ValueObjects;

namespace Sales.Application.Sagas;

/// <summary>
/// The checkout Saga orchestrator — the physical materialization of the DDD
/// <c>CheckoutApplicationService</c> (§6.4) and the orchestration Saga of
/// Microservices §3.3. <c>sales-service</c> owns the checkout flow end to end.
/// <para>
/// The flow is split in two halves:
/// <list type="bullet">
///   <item><b>Synchronous start</b> — <see cref="StartAsync"/>: reserve stock,
///   transition the cart, request payment.</item>
///   <item><b>Asynchronous continuation / compensation</b> — handled by the
///   <c>PagoAprobado</c> / <c>PagoRechazado</c> integration-event handlers.</item>
/// </list>
/// </para>
/// </summary>
public sealed class CheckoutSagaOrchestrator
{
    private readonly ICartRepository _carts;
    private readonly IStockReservationService _stock;
    private readonly IPaymentGatewayService _payments;
    private readonly CartTransitionService _transition;

    public CheckoutSagaOrchestrator(
        ICartRepository carts,
        IStockReservationService stock,
        IPaymentGatewayService payments,
        CartTransitionService transition)
    {
        _carts = carts;
        _stock = stock;
        _payments = payments;
        _transition = transition;
    }

    /// <summary>
    /// Runs the synchronous start of the checkout Saga for a cart.
    /// </summary>
    public async Task StartAsync(Guid cartId, string paymentMethod, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(cartId), cancellationToken)
            ?? throw new CartNotFoundException(cartId);

        var lines = BuildStockLines(cart);

        // Step 1 — reserve stock in catalog-service.
        await _stock.ReserveAsync(cart.Id, lines, cancellationToken);

        try
        {
            // Step 2 — domain checkout: validates the invariants (cart Active,
            // not empty, total > 0) and records the CheckoutIniciado event.
            cart.Checkout();
        }
        catch
        {
            // Compensation for step 1: the cart could not be checked out, so
            // release the stock that was just reserved before rethrowing.
            await _stock.ReleaseAsync(cart.Id, lines, cancellationToken);
            throw;
        }

        await _carts.SaveAsync(cart, cancellationToken);

        // Step 3 — request payment. One-way: the infrastructure adapter
        // publishes CheckoutIniciado to finance-service. The outcome returns
        // asynchronously as the PagoAprobado / PagoRechazado integration events.
        var order = _transition.PlaceOrder(cart);
        await _payments.RequestPaymentAsync(order, paymentMethod, cancellationToken);
    }

    private static IReadOnlyCollection<StockLine> BuildStockLines(ShoppingCart cart) =>
        cart.Items
            .Select(item => new StockLine(item.Product.ProductId, item.Quantity))
            .ToList();
}
