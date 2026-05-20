using Identity.Application.Abstractions;
using Identity.Application.Messaging;
using MediatR;

namespace Identity.Application.Behaviors;

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
