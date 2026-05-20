using Microsoft.Extensions.Caching.Memory;
using Sales.Domain.Aggregates;
using Sales.Domain.Interfaces;
using Sales.Domain.ValueObjects;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Caching;

/// <summary>
/// Caching layer for cart reads, implemented as a <b>Decorator</b> over
/// <see cref="CartRepository"/> (rubric: "Implementación de Caché"). It wraps the
/// real repository and adds an <see cref="IMemoryCache"/> read-through cache
/// without either the Domain or the inner repository knowing about caching.
/// <para>
/// On a cache hit the aggregate is re-attached to the current DbContext so writes
/// still work; on every <see cref="SaveAsync"/> the entry is invalidated, so the
/// cache never diverges from the database within an instance. Cross-instance
/// staleness under horizontal scaling is the documented evolution path to a
/// distributed (Redis) cache.
/// </para>
/// </summary>
public sealed class CachedCartRepository : ICartRepository
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);

    private readonly CartRepository _inner;
    private readonly IMemoryCache _cache;

    public CachedCartRepository(CartRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<ShoppingCart?> GetAsync(CartId id, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey(id), out ShoppingCart? cached) && cached is not null)
        {
            // Rebind the cached aggregate to this scope's DbContext before returning.
            _inner.Reattach(cached);
            return cached;
        }

        var cart = await _inner.GetAsync(id, cancellationToken);
        if (cart is not null)
        {
            _cache.Set(CacheKey(id), cart, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheLifetime,
            });
        }

        return cart;
    }

    public async Task SaveAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        await _inner.SaveAsync(cart, cancellationToken);

        // Write-through invalidation: the next read reloads fresh from the database.
        _cache.Remove(CacheKey(cart.Id));
    }

    private static string CacheKey(CartId id) => $"sales:cart:{id.Value}";
}
