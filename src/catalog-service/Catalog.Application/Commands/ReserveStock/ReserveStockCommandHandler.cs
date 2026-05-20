using Catalog.Application.Abstractions;
using Catalog.Application.Dtos;
using Catalog.Application.Exceptions;
using Catalog.Domain.Interfaces;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Commands.ReserveStock;

public sealed class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, Unit>
{
    private readonly IProductRepository _products;
    private readonly ICartStockReservationStore _reservations;

    public ReserveStockCommandHandler(
        IProductRepository products,
        ICartStockReservationStore reservations)
    {
        _products = products;
        _reservations = reservations;
    }

    public async Task<Unit> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var alreadyReserved = await _reservations.GetReservedAsync(request.CartId, cancellationToken);

        foreach (var line in request.Lines)
        {
            await ReserveLineAsync(
                request.CartId,
                line,
                alreadyReserved,
                cancellationToken);
        }

        return Unit.Value;
    }

    private async Task ReserveLineAsync(
        Guid cartId,
        StockLineDto line,
        IReadOnlyDictionary<Guid, int> alreadyReserved,
        CancellationToken cancellationToken)
    {
        var currentlyHeld = alreadyReserved.GetValueOrDefault(line.ProductId);
        var delta = line.Quantity - currentlyHeld;

        if (delta <= 0)
        {
            return;
        }

        var product = await _products.GetAsync(new ProductId(line.ProductId), cancellationToken)
            ?? throw new ProductNotFoundException(line.ProductId);

        product.Reserve(delta);

        await _products.SaveAsync(product, cancellationToken);
        await _reservations.UpsertAsync(cartId, line.ProductId, line.Quantity, cancellationToken);
    }
}
