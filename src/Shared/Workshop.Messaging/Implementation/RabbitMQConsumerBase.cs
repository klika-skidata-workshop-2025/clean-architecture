using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Workshop.Messaging.Configuration;

namespace Workshop.Messaging.Implementation;

/// <summary>
/// Base class for RabbitMQ consumers.
/// Inherit from this class to create a consumer that processes specific events.
/// </summary>
/// <remarks>
/// This base class handles:
/// - Connection management to RabbitMQ
/// - Queue declaration and binding
/// - Message deserialization
/// - Error handling and logging
/// - Graceful shutdown
///
/// Derived classes only need to:
/// 1. Specify QueueName and RoutingKeys
/// 2. Implement HandleMessageAsync to process messages
/// </remarks>
/// <example>
/// <code>
/// public class DeviceStatusConsumer : RabbitMQConsumerBase
/// {
///     protected override string QueueName => "monitoring.device.status";
///     protected override string[] RoutingKeys => new[] { "device.status.*" };
///
///     public DeviceStatusConsumer(
///         IOptions&lt;RabbitMQOptions&gt; options,
///         ILogger&lt;DeviceStatusConsumer&gt; logger)
///         : base(options, logger)
///     {
///     }
///
///     protected override async Task HandleMessageAsync(string message, string routingKey)
///     {
///         var evt = JsonSerializer.Deserialize&lt;DeviceStatusChangedEvent&gt;(message);
///         // Process event...
///     }
/// }
/// </code>
/// </example>
public abstract class RabbitMQConsumerBase : BackgroundService
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger _logger;
    private IConnection? _connection;
    private IModel? _channel;

    /// <summary>
    /// Name of the queue to consume from.
    /// This should be unique per consumer service.
    /// </summary>
    /// <example>
    /// "monitoring.device.events" - Queue name for monitoring service consuming device events
    /// </example>
    protected abstract string QueueName { get; }

    /// <summary>
    /// Routing keys to bind the queue to.
    /// Supports wildcard patterns for topic exchanges.
    /// </summary>
    /// <remarks>
    /// Wildcard patterns:
    /// - "*" matches exactly one word
    /// - "#" matches zero or more words
    ///
    /// Examples:
    /// - "device.status.changed" - Exact match
    /// - "device.status.*" - Match any device status event
    /// - "device.*" - Match any device event
    /// - "*.alert.*" - Match alert events from any service
    /// - "#" - Match all events (use sparingly!)
    /// </remarks>
    protected abstract string[] RoutingKeys { get; }

    /// <summary>
    /// Whether the queue should be durable (survive broker restarts).
    /// Default: true (recommended)
    /// </summary>
    protected virtual bool IsDurable => true;

    /// <summary>
    /// Whether the queue should be exclusive to this connection.
    /// Default: false (allows multiple consumers)
    /// </summary>
    protected virtual bool IsExclusive => false;

    /// <summary>
    /// Whether the queue should auto-delete when no longer used.
    /// Default: false (queue persists)
    /// </summary>
    protected virtual bool AutoDelete => false;

    /// <summary>
    /// Number of messages to prefetch.
    /// Higher values = better throughput, lower values = better load distribution.
    /// Default: 10
    /// </summary>
    protected virtual ushort PrefetchCount => 10;

    protected RabbitMQConsumerBase(
        IOptions<RabbitMQOptions> options,
        ILogger logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts the consumer background service.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() =>
        {
            _logger.LogInformation("RabbitMQ consumer {QueueName} is stopping", QueueName);
        });

        try
        {
            // Create connection
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange (idempotent)
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: _options.Durable);

            // Declare queue
            _channel.QueueDeclare(
                queue: QueueName,
                durable: IsDurable,
                exclusive: IsExclusive,
                autoDelete: AutoDelete,
                arguments: null);

            // Bind queue to exchange with routing keys
            foreach (var routingKey in RoutingKeys)
            {
                _channel.QueueBind(
                    queue: QueueName,
                    exchange: _options.ExchangeName,
                    routingKey: routingKey);

                _logger.LogInformation(
                    "Queue '{QueueName}' bound to exchange '{ExchangeName}' with routing key '{RoutingKey}'",
                    QueueName, _options.ExchangeName, routingKey);
            }

            // Set QoS (prefetch count)
            _channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: PrefetchCount,
                global: false);

            // Create consumer
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                await OnMessageReceivedAsync(ea, stoppingToken);
            };

            // Start consuming
            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,  // Manual acknowledgment for reliability
                consumer: consumer);

            _logger.LogInformation(
                "RabbitMQ consumer {QueueName} started successfully",
                QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to start RabbitMQ consumer {QueueName}",
                QueueName);
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a message is received from RabbitMQ.
    /// </summary>
    private async Task OnMessageReceivedAsync(
        BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
    {
        var routingKey = ea.RoutingKey;
        string? message = null;

        try
        {
            // Deserialize message
            var body = ea.Body.ToArray();
            message = Encoding.UTF8.GetString(body);

            _logger.LogDebug(
                "Received message on queue '{QueueName}' with routing key '{RoutingKey}'",
                QueueName, routingKey);

            // Process message (implemented by derived class)
            await HandleMessageAsync(message, routingKey, cancellationToken);

            // Acknowledge message (removes from queue)
            _channel?.BasicAck(ea.DeliveryTag, multiple: false);

            _logger.LogDebug(
                "Successfully processed message on queue '{QueueName}' with routing key '{RoutingKey}'",
                QueueName, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing message on queue '{QueueName}' with routing key '{RoutingKey}'. Message: {Message}",
                QueueName, routingKey, message);

            // Reject message and requeue (will be retried)
            // In production, you might want to use a dead-letter queue instead
            _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
        }
    }

    /// <summary>
    /// Handles a received message.
    /// Derived classes must implement this method to process messages.
    /// </summary>
    /// <param name="message">The message body (JSON string)</param>
    /// <param name="routingKey">The routing key the message was published with</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    protected abstract Task HandleMessageAsync(
        string message,
        string routingKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Helper method to deserialize JSON message to a specific type.
    /// </summary>
    protected T DeserializeMessage<T>(string message)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Deserialized message is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message to type {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Disposes RabbitMQ resources when the service stops.
    /// </summary>
    public override void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ consumer {QueueName} disposed", QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while disposing RabbitMQ consumer {QueueName}", QueueName);
        }

        base.Dispose();
    }
}
