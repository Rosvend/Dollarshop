using Catalog.Domain.Aggregates;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(Product product, CancellationToken cancellationToken = default);
}
