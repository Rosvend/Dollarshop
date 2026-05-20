using MediatR;

namespace Identity.Application.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
