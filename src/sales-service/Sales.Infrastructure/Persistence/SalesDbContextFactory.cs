using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sales.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools (<c>dotnet ef migrations</c>).
/// It lets migrations be generated offline — no running host and no live database
/// are needed — by supplying a self-contained PostgreSQL connection string. The
/// real connection string is provided by the host at run time (Phase 4).
/// </summary>
public sealed class SalesDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
{
    public SalesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=sales_db;Username=sales;Password=sales")
            .Options;

        return new SalesDbContext(options);
    }
}
