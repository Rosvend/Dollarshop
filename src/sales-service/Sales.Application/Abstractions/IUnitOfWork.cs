namespace Sales.Application.Abstractions;

/// <summary>
/// Represents the transaction boundary of a use case. The Application layer
/// declares this port; the Infrastructure layer implements it (Phase 3) over
/// the ORM. Committing also flushes the domain events recorded by the mutated
/// aggregates — written to the Outbox within the same transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Atomically persists all changes made during the current use case.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
