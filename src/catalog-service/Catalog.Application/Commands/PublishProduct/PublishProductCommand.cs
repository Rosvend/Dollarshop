using Catalog.Application.Messaging;

namespace Catalog.Application.Commands.PublishProduct;

public sealed record PublishProductCommand(
    string Sku,
    string Name,
    decimal ListPrice,
    string Currency,
    int InitialStock) : ICommand<Guid>;
