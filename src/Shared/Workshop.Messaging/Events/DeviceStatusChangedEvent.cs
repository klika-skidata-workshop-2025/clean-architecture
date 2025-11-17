using Workshop.Messaging.Abstractions;

namespace Workshop.Messaging.Events;

/// <summary>
/// Event published when a device's status changes.
/// This is the primary event from the Device Service consumed by other services.
/// </summary>
/// <remarks>
/// Published by: Device Service
/// Consumed by: Monitoring Service, Diagnostics Service
///
/// Routing key: "device.status.changed"
///
/// This event triggers monitoring rules and is logged for diagnostics.
/// </remarks>
/// <example>
/// Publishing:
/// <code>
/// await _publisher.PublishAsync(new DeviceStatusChangedEvent
/// {
///     DeviceId = "gate-42",
///     DeviceName = "Main Entrance Gate",
///     DeviceType = "GATE",
///     OldStatus = "ACTIVE",
///     NewStatus = "BLOCKED",
///     Reason = "Security alert triggered",
///     Location = "Building A - Entrance"
/// });
/// </code>
///
/// Consuming:
/// <code>
/// protected override async Task HandleMessageAsync(string message, string routingKey)
/// {
///     var evt = DeserializeMessage&lt;DeviceStatusChangedEvent&gt;(message);
///
///     if (evt.NewStatus == "BLOCKED")
///     {
///         // Trigger alert
///     }
/// }
/// </code>
/// </example>
public class DeviceStatusChangedEvent : IMessage
{
    /// <inheritdoc/>
    public string Topic => "device.status.changed";

    /// <inheritdoc/>
    public string Type => nameof(DeviceStatusChangedEvent);

    /// <inheritdoc/>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier of the device.
    /// </summary>
    /// <example>"gate-42", "lift-north-1", "counter-entrance-1"</example>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the device.
    /// </summary>
    /// <example>"Main Entrance Gate", "North Ski Lift"</example>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Type of device.
    /// </summary>
    /// <example>"GATE", "BARRIER", "TURNSTILE", "LIFT", "COUNTER", "READER"</example>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Previous status of the device.
    /// </summary>
    /// <example>"ACTIVE", "INACTIVE", "MAINTENANCE", "BLOCKED", "OFFLINE", "ERROR"</example>
    public string OldStatus { get; set; } = string.Empty;

    /// <summary>
    /// New status of the device.
    /// </summary>
    /// <example>"ACTIVE", "INACTIVE", "MAINTENANCE", "BLOCKED", "OFFLINE", "ERROR"</example>
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the status change.
    /// </summary>
    /// <example>"Manual update", "Heartbeat timeout", "Security alert"</example>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Physical location of the device.
    /// </summary>
    /// <example>"Building A - Main Entrance", "Ski Resort - North Slope"</example>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata about the status change.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
