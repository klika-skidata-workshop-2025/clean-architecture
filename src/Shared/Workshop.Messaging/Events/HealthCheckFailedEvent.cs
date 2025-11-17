using Workshop.Messaging.Abstractions;

namespace Workshop.Messaging.Events;

/// <summary>
/// Event published when a health check fails.
/// </summary>
/// <remarks>
/// Published by: Diagnostics Service
/// Consumed by: Monitoring Service (to create alerts), other services (for awareness)
///
/// Routing key: "diagnostics.health.failed"
///
/// This event indicates that a service or component is unhealthy.
/// </remarks>
public class HealthCheckFailedEvent : IMessage
{
    /// <inheritdoc/>
    public string Topic => "diagnostics.health.failed";

    /// <inheritdoc/>
    public string Type => nameof(HealthCheckFailedEvent);

    /// <inheritdoc/>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Name of the service or component that failed health check.
    /// </summary>
    /// <example>"DeviceService", "PostgreSQL", "RabbitMQ"</example>
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>
    /// Type of component.
    /// </summary>
    /// <example>"SERVICE", "DATABASE", "MESSAGE_BUS", "EXTERNAL_API"</example>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Health status.
    /// </summary>
    /// <example>"UNHEALTHY", "DEGRADED"</example>
    public string HealthStatus { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the health check failure.
    /// </summary>
    /// <example>"Connection timeout", "High error rate", "Service unavailable"</example>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Error message (if applicable).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Response time in milliseconds (if applicable).
    /// </summary>
    public double? ResponseTimeMs { get; set; }

    /// <summary>
    /// Whether this component is critical for system operation.
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// Additional metadata about the health check.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
