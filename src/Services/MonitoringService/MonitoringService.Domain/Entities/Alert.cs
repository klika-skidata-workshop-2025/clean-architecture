using MonitoringService.Domain.Common;
using MonitoringService.Domain.Enums;

namespace MonitoringService.Domain.Entities;

/// <summary>
/// Represents an alert triggered by a monitoring rule.
/// </summary>
public class Alert : BaseEntity
{
    /// <summary>
    /// Alert title/summary.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Detailed alert message.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Alert severity level.
    /// </summary>
    public Severity Severity { get; private set; }

    /// <summary>
    /// Current alert status.
    /// </summary>
    public AlertStatus Status { get; private set; }

    /// <summary>
    /// Device ID that triggered this alert (if applicable).
    /// </summary>
    public string? DeviceId { get; private set; }

    /// <summary>
    /// Device name that triggered this alert (if applicable).
    /// </summary>
    public string? DeviceName { get; private set; }

    /// <summary>
    /// Monitoring rule ID that triggered this alert (if applicable).
    /// </summary>
    public string? RuleId { get; private set; }

    /// <summary>
    /// Monitoring rule name that triggered this alert (if applicable).
    /// </summary>
    public string? RuleName { get; private set; }

    /// <summary>
    /// When the alert was acknowledged (UTC).
    /// </summary>
    public DateTime? AcknowledgedAt { get; private set; }

    /// <summary>
    /// User who acknowledged the alert.
    /// </summary>
    public string? AcknowledgedBy { get; private set; }

    /// <summary>
    /// When the alert was resolved (UTC).
    /// </summary>
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>
    /// Additional metadata in JSON format.
    /// </summary>
    public string Metadata { get; private set; } = "{}";

    // Private parameterless constructor for EF Core
    private Alert() { }

    /// <summary>
    /// Creates a new Alert instance.
    /// </summary>
    public static Alert Create(
        string title,
        string message,
        Severity severity,
        string? deviceId = null,
        string? deviceName = null,
        string? ruleId = null,
        string? ruleName = null)
    {
        return new Alert
        {
            Title = title,
            Message = message,
            Severity = severity,
            Status = AlertStatus.Active,
            DeviceId = deviceId,
            DeviceName = deviceName,
            RuleId = ruleId,
            RuleName = ruleName
        };
    }

    /// <summary>
    /// Acknowledges the alert.
    /// </summary>
    public void Acknowledge(string acknowledgedBy)
    {
        if (Status == AlertStatus.Acknowledged)
            return; // Already acknowledged

        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedBy = acknowledgedBy;
        MarkAsUpdated();
    }

    /// <summary>
    /// Resolves the alert.
    /// </summary>
    public void Resolve()
    {
        if (Status == AlertStatus.Resolved)
            return; // Already resolved

        Status = AlertStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the alert metadata.
    /// </summary>
    public void UpdateMetadata(string metadata)
    {
        Metadata = metadata;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if the alert is active (not acknowledged or resolved).
    /// </summary>
    public bool IsActive() => Status == AlertStatus.Active;

    /// <summary>
    /// Gets the age of the alert (time since creation).
    /// </summary>
    public TimeSpan GetAge() => DateTime.UtcNow - CreatedAt;
}
