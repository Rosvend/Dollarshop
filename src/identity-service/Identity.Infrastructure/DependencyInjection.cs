using Identity.Application.Abstractions;
using Identity.Domain.Interfaces;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Security;
using Identity.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? "Host=localhost;Port=5432;Database=identity_db;Username=sales;Password=sales";

        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordVerifier, Pbkdf2PasswordVerifier>();
        services.AddHostedService<IdentityDataSeeder>();

        return services;
    }
}
