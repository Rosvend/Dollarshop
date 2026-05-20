using Catalog.Domain.Aggregates;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Seed;

/// <summary>Seeds demo products when the catalog database is empty.</summary>
public sealed class CatalogDataSeeder : IHostedService
{
    private static readonly (Guid Id, string Sku, string Name, decimal Price, int Stock)[] DemoProducts =
    {
        (Guid.Parse("11111111-1111-1111-1111-111111111101"), "DS-001", "Wireless Mouse", 12.99m, 100),
        (Guid.Parse("11111111-1111-1111-1111-111111111102"), "DS-002", "USB-C Cable", 8.50m, 250),
        (Guid.Parse("11111111-1111-1111-1111-111111111103"), "DS-003", "Notebook A5", 3.99m, 500),
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CatalogDataSeeder> _logger;

    public CatalogDataSeeder(IServiceScopeFactory scopeFactory, ILogger<CatalogDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var demo in DemoProducts)
        {
            dbContext.Products.Add(new Product(
                new ProductId(demo.Id),
                new Sku(demo.Sku),
                new ProductName(demo.Name),
                new Money(demo.Price, Currency.USD),
                new StockLevel(demo.Stock)));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} demo products into catalog_db.", DemoProducts.Length);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
