using Catalog.Application.Dtos;
using Catalog.Application.Messaging;

namespace Catalog.Application.Queries.GetProduct;

public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductDto>;
