using Sales.Domain.Aggregates;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Interfaces;

/// <summary>
/// Persistence contract for the <see cref="ShoppingCart"/> aggregate. It is
/// declared in the Domain layer and implemented in Infrastructure, so the
/// domain depends on an abstraction it owns — never on a persistence library.
/// </summary>
public interface ICartRepository
{
    /// <summary>Loads a cart by its identity, or <c>null</c> if it does not exist.</summary>
    Task<ShoppingCart?> GetAsync(CartId id, CancellationToken cancellationToken = default);

    /// <summary>Persists the current state of a cart aggregate.</summary>
    Task SaveAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
}
