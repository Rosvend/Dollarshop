using Microsoft.EntityFrameworkCore;
using Sales.Domain.Aggregates;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;

namespace Sales.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the Domain port <see cref="ICartRepository"/>. It
/// returns the <see cref="ShoppingCart"/> aggregate fully loaded — no SQL,
/// <c>DbContext</c> or other technology leaks across the interface.
/// <para>
/// The actual database write is deferred to <c>IUnitOfWork.CommitAsync</c>: the
/// change tracker already holds the diff (added / modified / removed items), so
/// <see cref="SaveAsync"/> only needs to <c>Add</c> a brand-new aggregate.
/// </para>
/// </summary>
public sealed class CartRepository : ICartRepository
{
    private readonly SalesDbContext _dbContext;

    public CartRepository(SalesDbContext dbContext) => _dbContext = dbContext;

    public async Task<ShoppingCart?> GetAsync(CartId id, CancellationToken cancellationToken = default) =>
        await _dbContext.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);

    public Task SaveAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(cart).State == EntityState.Detached)
        {
            _dbContext.Carts.Add(cart);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Re-attaches an aggregate served from the cache to the current
    /// <see cref="SalesDbContext"/> as <c>Unchanged</c>. Because the cache is
    /// invalidated on every write, a cached cart always matches the database, so
    /// re-attaching is sound and lets subsequent domain behavior be change-tracked
    /// normally. Called by <c>CachedCartRepository</c> on a cache hit.
    /// </summary>
    public void Reattach(ShoppingCart cart)
    {
        if (_dbContext.Entry(cart).State == EntityState.Detached)
        {
            _dbContext.Attach(cart);
        }
    }
}
