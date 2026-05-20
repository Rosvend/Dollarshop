using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Infrastructure.Messaging;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Outbox;

/// <summary>
/// The Outbox relay (Microservices §3.4): a background worker that polls
/// <c>outbox_messages</c> for pending rows and publishes them to RabbitMQ.
/// <para>
/// A successfully published row is marked processed; a failed one keeps its
/// pending state and is retried on the next tick — making delivery
/// <b>at-least-once</b> (which is why every consumer is idempotent).
/// </para>
/// </summary>
public sealed class OutboxRelay : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqEventPublisher _publisher;
    private readonly ILogger<OutboxRelay> _logger;

    public OutboxRelay(
        IServiceScopeFactory scopeFactory,
        RabbitMqEventPublisher publisher,
        ILogger<OutboxRelay> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RelayPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox relay tick failed; will retry next interval.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RelayPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        var pending = await dbContext.Outbox
            .Where(message => message.ProcessedOn == null)
            .OrderBy(message => message.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        foreach (var message in pending)
        {
            try
            {
                await _publisher.PublishRawAsync(message.RoutingKey, message.Payload, cancellationToken);
                message.MarkProcessed();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message);
                _logger.LogWarning(ex, "Failed to relay outbox message {MessageId}.", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
