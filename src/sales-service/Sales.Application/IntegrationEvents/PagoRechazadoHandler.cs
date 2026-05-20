using MediatR;
using Sales.Application.Abstractions;
using Sales.Application.Exceptions;
using Sales.Domain.Aggregates;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.IntegrationEvents;

/// <summary>
/// Runs the compensating branch of the checkout Saga when payment is rejected.
/// Compensations are applied in <b>reverse order</b> of the Saga steps:
/// <list type="number">
///   <item>revert the checkout in the sales domain — records <c>CheckoutRevertido</c>;</item>
///   <item>release the stock reserved earlier in <c>catalog-service</c>.</item>
/// </list>
/// The handler is <b>idempotent</b>: a duplicate <see cref="PagoRechazado"/>
/// arriving after the cart has already been reverted is a safe no-op.
/// </summary>
public sealed class PagoRechazadoHandler : INotificationHandler<PagoRechazado>
{
    private readonly ICartRepository _carts;
    private readonly IStockReservationService _stock;
    private readonly IUnitOfWork _unitOfWork;

    public PagoRechazadoHandler(
        ICartRepository carts,
        IStockReservationService stock,
        IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _stock = stock;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PagoRechazado notification, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(notification.CartId), cancellationToken)
            ?? throw new CartNotFoundException(notification.CartId);

        // Idempotency guard: only a still-checked-out cart needs compensation.
        if (cart.Status != CartStatus.CheckedOut)
        {
            return;
        }

        var lines = cart.Items
            .Select(item => new StockLine(item.Product.ProductId, item.Quantity))
            .ToList();

        // Compensation 1 — revert the checkout in the sales domain.
        cart.RevertCheckout();

        // Compensation 2 — release the stock reserved in catalog-service.
        await _stock.ReleaseAsync(cart.Id, lines, cancellationToken);

        await _carts.SaveAsync(cart, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
