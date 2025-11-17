namespace MonitoringService.Domain.Enums;

/// <summary>
/// Represents the type of action to take when a monitoring rule is triggered.
/// </summary>
public enum RuleActionType
{
    /// <summary>
    /// Create an alert.
    /// </summary>
    CreateAlert = 1,

    /// <summary>
    /// Send a notification (email, SMS, etc.).
    /// </summary>
    SendNotification = 2,

    /// <summary>
    /// Execute a custom action/webhook.
    /// </summary>
    ExecuteCustomAction = 3
}
