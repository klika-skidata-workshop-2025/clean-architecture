namespace Workshop.Messaging.Configuration;

/// <summary>
/// Configuration options for RabbitMQ connection.
/// These settings are typically loaded from appsettings.json.
/// </summary>
/// <example>
/// appsettings.json:
/// <code>
/// {
///   "RabbitMQ": {
///     "Host": "localhost",
///     "Port": 5672,
///     "Username": "workshop",
///     "Password": "workshop123",
///     "VirtualHost": "/",
///     "ExchangeName": "workshop.events",
///     "ExchangeType": "topic"
///   }
/// }
/// </code>
/// </example>
public class RabbitMQOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ server hostname or IP address.
    /// Default: "localhost"
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ server port.
    /// Default: 5672 (AMQP protocol default port)
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Username for RabbitMQ authentication.
    /// Default: "guest" (not recommended for production)
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// Password for RabbitMQ authentication.
    /// Default: "guest" (not recommended for production)
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ virtual host.
    /// Virtual hosts provide logical separation between applications.
    /// Default: "/" (root virtual host)
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Name of the exchange to publish to and consume from.
    /// All workshop services use the same exchange for event distribution.
    /// Default: "workshop.events"
    /// </summary>
    public string ExchangeName { get; set; } = "workshop.events";

    /// <summary>
    /// Exchange type determines routing behavior.
    /// "topic" allows wildcard pattern matching on routing keys.
    /// Default: "topic"
    /// </summary>
    /// <remarks>
    /// Other types: "direct", "fanout", "headers"
    /// We use "topic" for flexible event routing (e.g., "device.*", "monitoring.alert.*")
    /// </remarks>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Whether exchanges and queues should survive broker restarts.
    /// Default: true (recommended for production)
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Connection timeout in seconds.
    /// Default: 30 seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Number of automatic connection recovery attempts.
    /// Default: 5
    /// </summary>
    public int RetryCount { get; set; } = 5;

    /// <summary>
    /// Delay between retry attempts in milliseconds.
    /// Default: 3000ms (3 seconds)
    /// </summary>
    public int RetryDelay { get; set; } = 3000;
}
