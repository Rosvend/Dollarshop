using MediatR;

namespace Identity.Application.Messaging;

public interface IBaseCommand
{
}

public interface ICommand : IRequest<Unit>, IBaseCommand
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand
{
}
