using Microsoft.EntityFrameworkCore;
using Sales.Application.Abstractions;
using Sales.Domain.Aggregates;
using Sales.Infrastructure.Outbox;

namespace Sales.Infrastructure.Persistence;

/// <summary>
/// The EF Core unit of work for <c>sales-service</c>. It owns the <c>sales-db</c>
/// session and implements the Application-layer <see cref="IUnitOfWork"/> port, so
/// the inner layers commit a use case without ever referencing EF Core.
/// <para>
/// <see cref="CommitAsync"/> implements the write side of the Transactional Outbox
/// (Microservices §3.4): it drains the domain events recorded by the mutated
/// aggregates into <c>outbox_messages</c> and then persists business rows and
/// outbox rows in a <b>single</b> <c>SaveChanges</c> transaction.
/// </para>
/// </summary>
public sealed class SalesDbContext : DbContext, IUnitOfWork
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {
    }

    public DbSet<ShoppingCart> Carts => Set<ShoppingCart>();

    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    /// <summary>
    /// Stages an integration message (e.g. the ACL payment request) into the
    /// Outbox so it commits atomically with the current use case.
    /// </summary>
    public void EnqueueIntegrationMessage(OutboxMessage message) => Outbox.Add(message);

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        DrainDomainEventsToOutbox();
        await SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
    }

    /// <summary>
    /// Moves every domain event recorded by a tracked aggregate into the Outbox
    /// table, then clears it from the aggregate — all before <c>SaveChanges</c>,
    /// so the events share the business transaction.
    /// </summary>
    private void DrainDomainEventsToOutbox()
    {
        var aggregates = ChangeTracker.Entries<ShoppingCart>()
            .Select(entry => entry.Entity)
            .Where(cart => cart.DomainEvents.Count > 0)
            .ToList();

        foreach (var cart in aggregates)
        {
            foreach (var domainEvent in cart.DomainEvents)
            {
                Outbox.Add(DomainEventOutboxFactory.FromDomainEvent(domainEvent));
            }

            cart.ClearDomainEvents();
        }
    }
}
