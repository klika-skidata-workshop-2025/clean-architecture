namespace Workshop.Messaging.Abstractions;

/// <summary>
/// Base interface for all event messages published to RabbitMQ.
/// All events in the workshop system must implement this interface.
/// </summary>
/// <remarks>
/// The Topic property is used for RabbitMQ routing keys, allowing consumers to subscribe
/// to specific types of events using pattern matching (e.g., "device.*" or "device.status.*").
/// </remarks>
/// <example>
/// <code>
/// public class DeviceStatusChangedEvent : IMessage
/// {
///     public string Topic => "device.status.changed";
///     public string Type => nameof(DeviceStatusChangedEvent);
///     public DateTime Timestamp { get; set; } = DateTime.UtcNow;
///
///     public string DeviceId { get; set; }
///     public string Status { get; set; }
/// }
/// </code>
/// </example>
public interface IMessage
{
    /// <summary>
    /// The routing topic for this message.
    /// Used by RabbitMQ to route messages to the correct consumers.
    /// </summary>
    /// <remarks>
    /// Topic format: "{domain}.{entity}.{action}"
    /// Examples:
    /// - "device.status.changed"
    /// - "monitoring.alert.triggered"
    /// - "diagnostics.health.failed"
    /// </remarks>
    string Topic { get; }

    /// <summary>
    /// The message type name.
    /// Typically the class name of the event.
    /// Used for deserialization and logging.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// When the event occurred (UTC).
    /// Set automatically to DateTime.UtcNow when the event is created.
    /// </summary>
    DateTime Timestamp { get; }
}
