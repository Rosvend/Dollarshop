using Catalog.Application.Dtos;
using Catalog.Application.Messaging;

namespace Catalog.Application.Commands.ReleaseStock;

public sealed record ReleaseStockCommand(Guid CartId, IReadOnlyList<StockLineDto> Lines) : ICommand;
