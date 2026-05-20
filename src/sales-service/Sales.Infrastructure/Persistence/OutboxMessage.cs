namespace Sales.Infrastructure.Persistence;

/// <summary>
/// A row of the Transactional Outbox (Microservices §3.4). Every integration
/// message — a sales domain event captured on commit, or an ACL payment request —
/// is written here <b>in the same database transaction</b> as the business change,
/// eliminating the dual-write problem. The <c>OutboxRelay</c> later publishes
/// pending rows to RabbitMQ and marks them processed.
/// </summary>
public sealed class OutboxMessage
{
    // Parameterless constructor for EF Core materialization.
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>When the originating event/request occurred.</summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>Logical message name (the domain event type or the ACL contract).</summary>
    public string MessageType { get; private set; } = default!;

    /// <summary>Topic-exchange routing key the relay publishes the message with.</summary>
    public string RoutingKey { get; private set; } = default!;

    /// <summary>The serialized JSON body.</summary>
    public string Payload { get; private set; } = default!;

    /// <summary>When the relay successfully published the message; <c>null</c> while pending.</summary>
    public DateTime? ProcessedOn { get; private set; }

    /// <summary>How many times publishing has been attempted and failed.</summary>
    public int RetryCount { get; private set; }

    /// <summary>The last failure reason, if any.</summary>
    public string? Error { get; private set; }

    public static OutboxMessage Create(string messageType, string routingKey, string payload, DateTime occurredOn) =>
        new()
        {
            Id = Guid.NewGuid(),
            MessageType = messageType,
            RoutingKey = routingKey,
            Payload = payload,
            OccurredOn = occurredOn,
        };

    /// <summary>Marks the message as successfully published.</summary>
    public void MarkProcessed() => ProcessedOn = DateTime.UtcNow;

    /// <summary>Records a failed publish attempt; the message stays pending for retry.</summary>
    public void MarkFailed(string error)
    {
        RetryCount++;
        Error = error;
    }
}
