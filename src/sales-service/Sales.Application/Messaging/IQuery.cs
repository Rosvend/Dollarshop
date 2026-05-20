using MediatR;

namespace Sales.Application.Messaging;

/// <summary>
/// A read-only query. Queries never mutate state and are not wrapped by the
/// transaction behavior.
/// </summary>
/// <typeparam name="TResponse">The type of the query result.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
