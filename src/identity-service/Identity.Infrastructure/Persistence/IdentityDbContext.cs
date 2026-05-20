using Identity.Application.Abstractions;
using Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext, IUnitOfWork
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
