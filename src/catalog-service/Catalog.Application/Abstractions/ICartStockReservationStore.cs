namespace Catalog.Application.Abstractions;

/// <summary>
/// Tracks how many units of each product are reserved for a checkout cart.
/// Enables idempotent reserve/release operations for the sales Saga.
/// </summary>
public interface ICartStockReservationStore
{
    Task<IReadOnlyDictionary<Guid, int>> GetReservedAsync(
        Guid cartId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        Guid cartId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken = default);
}
