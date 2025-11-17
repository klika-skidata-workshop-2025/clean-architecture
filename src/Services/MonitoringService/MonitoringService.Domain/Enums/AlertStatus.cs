namespace MonitoringService.Domain.Enums;

/// <summary>
/// Represents the status of an alert.
/// </summary>
public enum AlertStatus
{
    /// <summary>
    /// Alert is active and not yet acknowledged.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Alert has been acknowledged by an operator.
    /// </summary>
    Acknowledged = 2,

    /// <summary>
    /// Alert has been resolved.
    /// </summary>
    Resolved = 3
}
