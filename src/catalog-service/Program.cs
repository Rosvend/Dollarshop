var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapHealthChecks("/health");

// Stock reservation endpoints — the body shape matches sales' CatalogStockClient,
// but we only inspect it for logging; the demo stub always succeeds.
app.MapPost("/stock-reservations", (StockReservationRequest request, ILogger<Program> logger) =>
{
    logger.LogInformation(
        "Reserving stock for cart {CartId} ({LineCount} lines)",
        request.CartId,
        request.Lines.Count);
    return Results.Ok();
});

app.MapPost("/stock-reservations/release", (StockReservationRequest request, ILogger<Program> logger) =>
{
    logger.LogInformation(
        "Releasing stock for cart {CartId} ({LineCount} lines)",
        request.CartId,
        request.Lines.Count);
    return Results.Ok();
});

app.Run();

internal sealed record StockReservationRequest(Guid CartId, IReadOnlyList<StockLineDto> Lines);

internal sealed record StockLineDto(Guid ProductId, int Quantity);
