namespace Identity.Application.Abstractions;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
