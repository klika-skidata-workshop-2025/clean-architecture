namespace MonitoringService.Domain.Enums;

/// <summary>
/// Represents the type of condition in a monitoring rule.
/// </summary>
public enum RuleConditionType
{
    /// <summary>
    /// Device status equals a specific value.
    /// </summary>
    DeviceStatusEquals = 1,

    /// <summary>
    /// Device has been offline for a duration.
    /// </summary>
    DeviceOfflineForDuration = 2,

    /// <summary>
    /// Device event type matches a pattern.
    /// </summary>
    DeviceEventTypeMatches = 3,

    /// <summary>
    /// Custom expression evaluation.
    /// </summary>
    CustomExpression = 4
}
