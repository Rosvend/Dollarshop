using Catalog.Domain.Aggregates;
using Catalog.Domain.Interfaces;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext) => _dbContext = dbContext;

    public async Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken = default) =>
        await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Products
            .OrderBy(product => product.Name.Value)
            .ToListAsync(cancellationToken);

    public Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(product).State == EntityState.Detached)
        {
            _dbContext.Products.Add(product);
        }

        return Task.CompletedTask;
    }
}
