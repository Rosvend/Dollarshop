using Sales.Domain.ValueObjects;

namespace Sales.Application.Abstractions;

/// <summary>A single product line whose stock must be reserved or released.</summary>
public sealed record StockLine(ProductId ProductId, Quantity Quantity);

/// <summary>
/// Outbound port to <c>catalog-service</c> for stock reservation. The Application
/// layer declares it; an Infrastructure adapter (Phase 3) implements it over REST.
/// Both operations are part of the checkout Saga — <see cref="ReleaseAsync"/> is
/// the compensating action for <see cref="ReserveAsync"/>.
/// </summary>
public interface IStockReservationService
{
    /// <summary>Reserves stock for every line of a cart about to be checked out.</summary>
    Task ReserveAsync(
        CartId cartId,
        IReadOnlyCollection<StockLine> lines,
        CancellationToken cancellationToken = default);

    /// <summary>Releases a previously made reservation (compensating action).</summary>
    Task ReleaseAsync(
        CartId cartId,
        IReadOnlyCollection<StockLine> lines,
        CancellationToken cancellationToken = default);
}
