namespace Workshop.Messaging.Abstractions;

/// <summary>
/// Interface for publishing messages to RabbitMQ.
/// This abstraction allows for easy testing and different implementations.
/// </summary>
/// <remarks>
/// The publisher uses a topic exchange pattern where messages are routed based on
/// the message's Topic property. Consumers can subscribe to topics using wildcard patterns.
/// </remarks>
/// <example>
/// <code>
/// public class DeviceCommandHandler
/// {
///     private readonly IRabbitMQPublisher _publisher;
///
///     public async Task Handle(UpdateDeviceCommand command)
///     {
///         // ... update device logic
///
///         await _publisher.PublishAsync(new DeviceStatusChangedEvent
///         {
///             DeviceId = command.DeviceId,
///             Status = command.NewStatus
///         });
///     }
/// }
/// </code>
/// </example>
public interface IRabbitMQPublisher
{
    /// <summary>
    /// Publishes a message to RabbitMQ asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of message, must implement IMessage</typeparam>
    /// <param name="message">The message to publish</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>A task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when message is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when RabbitMQ connection is not available</exception>
    /// <example>
    /// <code>
    /// await _publisher.PublishAsync(new DeviceStatusChangedEvent
    /// {
    ///     DeviceId = "gate-42",
    ///     Status = "ACTIVE"
    /// });
    /// </code>
    /// </example>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IMessage;

    /// <summary>
    /// Checks if the publisher is connected to RabbitMQ.
    /// </summary>
    /// <returns>True if connected, false otherwise</returns>
    bool IsConnected { get; }
}
