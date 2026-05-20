using Catalog.Application.Dtos;
using Catalog.Application.Messaging;

namespace Catalog.Application.Queries.ListProducts;

public sealed record ListProductsQuery : IQuery<IReadOnlyList<ProductSummaryDto>>;
