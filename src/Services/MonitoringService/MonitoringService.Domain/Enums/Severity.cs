namespace MonitoringService.Domain.Enums;

/// <summary>
/// Represents the severity level of an alert or issue.
/// </summary>
public enum Severity
{
    /// <summary>
    /// Informational message, no action required.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning that should be monitored.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error requiring attention.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Critical issue requiring immediate action.
    /// </summary>
    Critical = 4
}
