using MediatR;
using Sales.Application.Abstractions;
using Sales.Application.Messaging;

namespace Sales.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps every command (<see cref="IBaseCommand"/>)
/// in a transaction boundary. The handler stages its changes; this behavior
/// commits them once via <see cref="IUnitOfWork"/>. If the handler throws, the
/// commit is skipped — the staged changes are never persisted (rollback).
/// <para>Queries do not implement <see cref="IBaseCommand"/>, so they are not
/// wrapped — the generic constraint excludes them.</para>
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IBaseCommand
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        await _unitOfWork.CommitAsync(cancellationToken);

        return response;
    }
}
