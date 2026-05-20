using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Behaviors;
using Sales.Application.Sagas;
using Sales.Domain.Services;

namespace Sales.Application;

/// <summary>
/// Composition root of the Application layer. The host (the External/API layer,
/// Phase 4) calls <see cref="AddSalesApplication"/>; the Infrastructure layer
/// (Phase 3) separately registers the implementations of <c>ICartRepository</c>,
/// <c>IUnitOfWork</c>, <c>IStockReservationService</c> and <c>IPaymentGatewayService</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddSalesApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR: command/query handlers and integration-event handlers.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation: one validator per command/query.
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Pipeline behaviors — order matters: validate first, then transact.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Pure domain services (stateless) and the checkout Saga orchestrator.
        services.AddSingleton<CartTransitionService>();
        services.AddSingleton<CartPricingService>();
        services.AddScoped<CheckoutSagaOrchestrator>();

        return services;
    }
}
