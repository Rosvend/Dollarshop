using MediatR;

namespace Sales.Application.Messaging;

/// <summary>
/// Non-generic marker shared by every command, used to target the
/// transaction pipeline behavior at commands (and not at queries).
/// </summary>
public interface IBaseCommand
{
}

/// <summary>A command that performs a single business action and returns no value.</summary>
public interface ICommand : IRequest<Unit>, IBaseCommand
{
}

/// <summary>A command that performs a single business action and returns a result.</summary>
/// <typeparam name="TResponse">The type of the result.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand
{
}
