using Identity.Domain.Aggregates;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Seed;

/// <summary>Seeds a demo customer used in local flows and docker compose.</summary>
public sealed class IdentityDataSeeder : IHostedService
{
    public static readonly Guid DemoCustomerId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public const string DemoUsername = "demo";
    public const string DemoPassword = "demo123";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(IServiceScopeFactory scopeFactory, ILogger<IdentityDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var passwordVerifier = scope.ServiceProvider.GetRequiredService<IPasswordVerifier>();

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var demoUser = new User(
            new UserId(DemoCustomerId),
            new Username(DemoUsername),
            passwordVerifier.Hash(DemoPassword),
            new RegistrationData(
                new Email("demo@dollarshop.test"),
                new PersonName("Demo Customer"),
                new PhoneNumber("+57 300 555 1234")));

        dbContext.Users.Add(demoUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded demo customer {UserId} (username: {Username}).",
            DemoCustomerId,
            DemoUsername);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
