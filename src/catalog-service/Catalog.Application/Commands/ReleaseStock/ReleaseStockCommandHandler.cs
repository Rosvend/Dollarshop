using Catalog.Application.Abstractions;
using Catalog.Application.Dtos;
using Catalog.Application.Exceptions;
using Catalog.Domain.Interfaces;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Commands.ReleaseStock;

public sealed class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand, Unit>
{
    private readonly IProductRepository _products;
    private readonly ICartStockReservationStore _reservations;

    public ReleaseStockCommandHandler(
        IProductRepository products,
        ICartStockReservationStore reservations)
    {
        _products = products;
        _reservations = reservations;
    }

    public async Task<Unit> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        var alreadyReserved = await _reservations.GetReservedAsync(request.CartId, cancellationToken);

        foreach (var line in request.Lines)
        {
            await ReleaseLineAsync(
                request.CartId,
                line,
                alreadyReserved,
                cancellationToken);
        }

        return Unit.Value;
    }

    private async Task ReleaseLineAsync(
        Guid cartId,
        StockLineDto line,
        IReadOnlyDictionary<Guid, int> alreadyReserved,
        CancellationToken cancellationToken)
    {
        if (!alreadyReserved.TryGetValue(line.ProductId, out var held) || held <= 0)
        {
            return;
        }

        var quantityToRelease = Math.Min(line.Quantity, held);

        var product = await _products.GetAsync(new ProductId(line.ProductId), cancellationToken)
            ?? throw new ProductNotFoundException(line.ProductId);

        product.Release(quantityToRelease);

        await _products.SaveAsync(product, cancellationToken);

        var remaining = held - quantityToRelease;
        if (remaining <= 0)
        {
            await _reservations.RemoveAsync(cartId, line.ProductId, cancellationToken);
        }
        else
        {
            await _reservations.UpsertAsync(cartId, line.ProductId, remaining, cancellationToken);
        }
    }
}
