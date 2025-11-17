using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Configuration;

namespace Workshop.Messaging.Implementation;

/// <summary>
/// Implementation of IRabbitMQPublisher using RabbitMQ.Client.
/// Publishes messages to a topic exchange for event-driven communication.
/// </summary>
/// <remarks>
/// This implementation:
/// - Uses a persistent connection to RabbitMQ
/// - Declares the exchange on startup
/// - Serializes messages to JSON
/// - Uses the message's Topic property as the routing key
/// - Publishes messages with persistent delivery mode
/// </remarks>
public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of RabbitMQPublisher.
    /// </summary>
    /// <param name="options">RabbitMQ configuration options</param>
    /// <param name="logger">Logger for diagnostics</param>
    public RabbitMQPublisher(
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQPublisher> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            // Create connection factory
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,  // Automatic connection recovery
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeout)
            };

            _logger.LogInformation(
                "Connecting to RabbitMQ at {Host}:{Port} (VHost: {VirtualHost})",
                _options.Host, _options.Port, _options.VirtualHost);

            // Create connection and channel
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the topic exchange
            // This is idempotent - if the exchange exists, this does nothing
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: _options.Durable,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation(
                "Successfully connected to RabbitMQ. Exchange '{ExchangeName}' declared.",
                _options.ExchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to connect to RabbitMQ at {Host}:{Port}",
                _options.Host, _options.Port);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool IsConnected => _connection?.IsOpen ?? false;

    /// <inheritdoc/>
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : IMessage
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (!IsConnected)
        {
            throw new InvalidOperationException(
                "Cannot publish message: RabbitMQ connection is not available");
        }

        try
        {
            // Serialize message to JSON
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var body = Encoding.UTF8.GetBytes(json);

            // Set message properties
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;  // Survive broker restarts
            properties.ContentType = "application/json";
            properties.ContentEncoding = "utf-8";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = message.Type;  // Message type for debugging
            properties.MessageId = Guid.NewGuid().ToString();

            // Add custom headers
            properties.Headers = new Dictionary<string, object>
            {
                { "topic", message.Topic },
                { "timestamp", message.Timestamp.ToString("o") },
                { "type", message.Type }
            };

            // Publish to exchange using the message's topic as routing key
            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: message.Topic,
                basicProperties: properties,
                body: body);

            _logger.LogDebug(
                "Published message {MessageType} to topic '{Topic}' (MessageId: {MessageId})",
                message.Type, message.Topic, properties.MessageId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish message {MessageType} to topic '{Topic}'",
                message.Type, message.Topic);
            throw;
        }
    }

    /// <summary>
    /// Disposes the RabbitMQ connection and channel.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while disposing RabbitMQ connection");
        }

        _disposed = true;
    }
}
