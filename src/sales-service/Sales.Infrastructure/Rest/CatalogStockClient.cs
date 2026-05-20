using System.Net.Http.Json;
using Sales.Application.Abstractions;
using Sales.Domain.ValueObjects;
using Sales.Infrastructure.Rest.Contracts;

namespace Sales.Infrastructure.Rest;

/// <summary>
/// REST adapter implementing the Application port <see cref="IStockReservationService"/>
/// over HTTP toward <c>catalog-service</c> (Microservices §3.1, Customer/Supplier).
/// <para>
/// The <see cref="HttpClient"/> is supplied by <c>IHttpClientFactory</c> with the
/// standard resilience handler attached in <c>DependencyInjection</c> — retry with
/// exponential backoff, circuit breaker and a concurrency limiter (§3.5). The
/// client itself stays free of resilience plumbing.
/// </para>
/// </summary>
public sealed class CatalogStockClient : IStockReservationService
{
    private readonly HttpClient _httpClient;

    public CatalogStockClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task ReserveAsync(
        CartId cartId,
        IReadOnlyCollection<StockLine> lines,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "stock-reservations", ToRequest(cartId, lines), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReleaseAsync(
        CartId cartId,
        IReadOnlyCollection<StockLine> lines,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "stock-reservations/release", ToRequest(cartId, lines), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static StockReservationRequest ToRequest(CartId cartId, IReadOnlyCollection<StockLine> lines) =>
        new(
            cartId.Value,
            lines.Select(line => new StockLineDto(line.ProductId.Value, line.Quantity.Value)).ToList());
}
