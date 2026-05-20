using Sales.Application.Dtos;
using Sales.Application.Messaging;

namespace Sales.Application.Queries.GetCart;

/// <summary>Read-side use case: fetch the current state of a cart.</summary>
public sealed record GetCartQuery(Guid CartId) : IQuery<CartDto>;
