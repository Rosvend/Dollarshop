namespace Sales.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ connection and topology settings, bound from the host configuration
/// section <c>RabbitMq</c>. Defaults target a local broker for development.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    /// <summary>The shared topic exchange every Dollarshop event flows through (§3.2).</summary>
    public string Exchange { get; set; } = "dollarshop.events";

    /// <summary>Queue this service consumes Finance's payment results from.</summary>
    public string PaymentResultQueue { get; set; } = "sales.payment-results";

    /// <summary>Routing key Finance publishes payment results with.</summary>
    public string PaymentResultRoutingKey { get; set; } = "sales.payment.result";
}
