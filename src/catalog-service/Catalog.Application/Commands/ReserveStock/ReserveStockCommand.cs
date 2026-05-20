using Catalog.Application.Dtos;
using Catalog.Application.Messaging;

namespace Catalog.Application.Commands.ReserveStock;

public sealed record ReserveStockCommand(Guid CartId, IReadOnlyList<StockLineDto> Lines) : ICommand;
