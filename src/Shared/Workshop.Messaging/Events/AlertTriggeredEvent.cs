using Workshop.Messaging.Abstractions;

namespace Workshop.Messaging.Events;

/// <summary>
/// Event published when a monitoring alert is triggered.
/// </summary>
/// <remarks>
/// Published by: Monitoring Service
/// Consumed by: Diagnostics Service, Device Service (for automated responses)
///
/// Routing key: "monitoring.alert.triggered"
///
/// This event notifies other services when monitoring rules detect issues.
/// </remarks>
public class AlertTriggeredEvent : IMessage
{
    /// <inheritdoc/>
    public string Topic => "monitoring.alert.triggered";

    /// <inheritdoc/>
    public string Type => nameof(AlertTriggeredEvent);

    /// <inheritdoc/>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier of the alert.
    /// </summary>
    public string AlertId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the device that triggered the alert.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the device.
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Severity of the alert.
    /// </summary>
    /// <example>"INFO", "WARNING", "ERROR", "CRITICAL"</example>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Title of the alert.
    /// </summary>
    /// <example>"Device Offline", "High Error Rate", "Security Breach"</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the alert.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ID of the monitoring rule that triggered this alert.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the monitoring rule.
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata about the alert.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
