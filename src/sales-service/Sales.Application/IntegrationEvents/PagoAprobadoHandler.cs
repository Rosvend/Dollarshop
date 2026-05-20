using MediatR;
using Sales.Application.Abstractions;
using Sales.Application.Exceptions;
using Sales.Domain.Aggregates;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Application.IntegrationEvents;

/// <summary>
/// Continues the checkout Saga when payment is approved: it closes the sale on
/// the cart aggregate (which records <c>VentaCerrada</c>).
/// <para>
/// The handler is <b>idempotent</b> — under the broker's at-least-once delivery
/// (§3.4) a duplicate <see cref="PagoAprobado"/> arriving after the sale is
/// already closed is a safe no-op. Notification handlers are outside the command
/// pipeline, so this one owns its transaction explicitly.
/// </para>
/// </summary>
public sealed class PagoAprobadoHandler : INotificationHandler<PagoAprobado>
{
    private readonly ICartRepository _carts;
    private readonly IUnitOfWork _unitOfWork;

    public PagoAprobadoHandler(ICartRepository carts, IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PagoAprobado notification, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetAsync(new CartId(notification.CartId), cancellationToken)
            ?? throw new CartNotFoundException(notification.CartId);

        // Idempotency guard: only a still-checked-out cart needs to be closed.
        if (cart.Status != CartStatus.CheckedOut)
        {
            return;
        }

        cart.ConfirmSale();

        await _carts.SaveAsync(cart, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
