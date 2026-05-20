using MediatR;

namespace Catalog.Application.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
