using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sales.Application.Abstractions;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Acl;
using Sales.Infrastructure.Caching;
using Sales.Infrastructure.Messaging;
using Sales.Infrastructure.Outbox;
using Sales.Infrastructure.Persistence;
using Sales.Infrastructure.Rest;

namespace Sales.Infrastructure;

/// <summary>
/// Composition root of the Infrastructure layer. The host (the External/API layer,
/// Phase 4) calls <see cref="AddSalesInfrastructure"/> alongside
/// <c>AddSalesApplication</c>; together they wire every port the inner layers
/// declared to its concrete adapter.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddMessaging(services, configuration);
        AddCatalogRestClient(services, configuration);

        return services;
    }

    /// <summary>EF Core persistence, the repository and its caching decorator.</summary>
    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SalesDb")
            ?? "Host=localhost;Port=5432;Database=sales_db;Username=sales;Password=sales";

        services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(connectionString));

        // The DbContext IS the unit of work — same scoped instance.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SalesDbContext>());

        // Repository + Decorator: ICartRepository resolves to the cache-aware wrapper.
        services.AddScoped<CartRepository>();
        services.AddMemoryCache();
        services.AddScoped<ICartRepository>(sp => new CachedCartRepository(
            sp.GetRequiredService<CartRepository>(),
            sp.GetRequiredService<IMemoryCache>()));

        // The outbound Finance ACL implements the payment port.
        services.AddScoped<IPaymentGatewayService, FinancePaymentGateway>();
    }

    /// <summary>RabbitMQ, the Outbox relay and the inbound Finance ACL consumer.</summary>
    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<RabbitMqEventPublisher>();
        services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<RabbitMqEventPublisher>());

        services.AddHostedService<OutboxRelay>();
        services.AddHostedService<PaymentResultConsumer>();
    }

    /// <summary>The resilient REST client to <c>catalog-service</c> (§3.1, §3.5).</summary>
    private static void AddCatalogRestClient(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CatalogOptions>(configuration.GetSection(CatalogOptions.SectionName));

        services
            .AddHttpClient<IStockReservationService, CatalogStockClient>((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<CatalogOptions>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseUrl);
            })
            // One handler bundles the §3.5 resilience patterns: retry with
            // exponential backoff, circuit breaker, per-attempt timeout and a
            // concurrency limiter (bulkhead).
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
            });
    }
}
