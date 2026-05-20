using Catalog.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class CartStockReservationStore : ICartStockReservationStore
{
    private readonly CatalogDbContext _dbContext;

    public CartStockReservationStore(CatalogDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyDictionary<Guid, int>> GetReservedAsync(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.CartStockReservations
            .Where(row => row.CartId == cartId)
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.ProductId, row => row.Quantity);
    }

    public async Task UpsertAsync(
        Guid cartId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.CartStockReservations
            .FirstOrDefaultAsync(
                row => row.CartId == cartId && row.ProductId == productId,
                cancellationToken);

        if (existing is null)
        {
            _dbContext.CartStockReservations.Add(new CartStockReservation
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
            });
        }
        else
        {
            existing.Quantity = quantity;
        }
    }

    public async Task RemoveAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.CartStockReservations
            .FirstOrDefaultAsync(
                row => row.CartId == cartId && row.ProductId == productId,
                cancellationToken);

        if (existing is not null)
        {
            _dbContext.CartStockReservations.Remove(existing);
        }
    }
}
