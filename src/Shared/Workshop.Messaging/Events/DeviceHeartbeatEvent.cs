using Workshop.Messaging.Abstractions;

namespace Workshop.Messaging.Events;

/// <summary>
/// Event published periodically by devices to indicate they are alive and healthy.
/// </summary>
/// <remarks>
/// Published by: Device Service
/// Consumed by: Diagnostics Service (for health tracking)
///
/// Routing key: "device.heartbeat"
///
/// This event is published periodically (e.g., every 30 seconds) to track device availability.
/// Missing heartbeats can trigger "device offline" alerts.
/// </remarks>
public class DeviceHeartbeatEvent : IMessage
{
    /// <inheritdoc/>
    public string Topic => "device.heartbeat";

    /// <inheritdoc/>
    public string Type => nameof(DeviceHeartbeatEvent);

    /// <inheritdoc/>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier of the device.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the device.
    /// </summary>
    /// <example>"ACTIVE", "MAINTENANCE", "OFFLINE"</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Device type.
    /// </summary>
    /// <example>"GATE", "LIFT", "COUNTER"</example>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Physical location of the device.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Health data from the device (optional).
    /// Can include metrics like CPU usage, memory, temperature, etc.
    /// </summary>
    public Dictionary<string, string> HealthData { get; set; } = new();
}
