using MonitoringService.Domain.Common;
using MonitoringService.Domain.Enums;

namespace MonitoringService.Domain.Entities;

/// <summary>
/// Represents a monitoring rule that triggers alerts based on conditions.
/// </summary>
public class MonitoringRule : BaseEntity
{
    /// <summary>
    /// Rule name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Rule description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Whether the rule is enabled.
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// Type of condition to evaluate.
    /// </summary>
    public RuleConditionType ConditionType { get; private set; }

    /// <summary>
    /// Condition expression/value (JSON format for complex conditions).
    /// </summary>
    public string ConditionValue { get; private set; } = string.Empty;

    /// <summary>
    /// Type of action to take when the rule is triggered.
    /// </summary>
    public RuleActionType ActionType { get; private set; }

    /// <summary>
    /// Action configuration (JSON format).
    /// </summary>
    public string ActionConfig { get; private set; } = "{}";

    /// <summary>
    /// Alert severity to use when creating alerts.
    /// </summary>
    public Severity AlertSeverity { get; private set; }

    /// <summary>
    /// Device ID filter (null = applies to all devices).
    /// </summary>
    public string? DeviceIdFilter { get; private set; }

    /// <summary>
    /// Device type filter (null = applies to all types).
    /// </summary>
    public string? DeviceTypeFilter { get; private set; }

    /// <summary>
    /// Last time this rule was triggered (UTC).
    /// </summary>
    public DateTime? LastTriggeredAt { get; private set; }

    /// <summary>
    /// Number of times this rule has been triggered.
    /// </summary>
    public int TriggerCount { get; private set; }

    // Private parameterless constructor for EF Core
    private MonitoringRule() { }

    /// <summary>
    /// Creates a new MonitoringRule instance.
    /// </summary>
    public static MonitoringRule Create(
        string name,
        string description,
        RuleConditionType conditionType,
        string conditionValue,
        RuleActionType actionType,
        Severity alertSeverity,
        string? deviceIdFilter = null,
        string? deviceTypeFilter = null)
    {
        return new MonitoringRule
        {
            Name = name,
            Description = description,
            IsEnabled = true,
            ConditionType = conditionType,
            ConditionValue = conditionValue,
            ActionType = actionType,
            AlertSeverity = alertSeverity,
            DeviceIdFilter = deviceIdFilter,
            DeviceTypeFilter = deviceTypeFilter,
            TriggerCount = 0
        };
    }

    /// <summary>
    /// Updates the rule configuration.
    /// </summary>
    public void Update(
        string? name = null,
        string? description = null,
        RuleConditionType? conditionType = null,
        string? conditionValue = null,
        RuleActionType? actionType = null,
        Severity? alertSeverity = null,
        string? deviceIdFilter = null,
        string? deviceTypeFilter = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (conditionType.HasValue)
            ConditionType = conditionType.Value;

        if (!string.IsNullOrWhiteSpace(conditionValue))
            ConditionValue = conditionValue;

        if (actionType.HasValue)
            ActionType = actionType.Value;

        if (alertSeverity.HasValue)
            AlertSeverity = alertSeverity.Value;

        if (deviceIdFilter != null)
            DeviceIdFilter = deviceIdFilter;

        if (deviceTypeFilter != null)
            DeviceTypeFilter = deviceTypeFilter;

        MarkAsUpdated();
    }

    /// <summary>
    /// Enables the rule.
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Disables the rule.
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records that the rule was triggered.
    /// </summary>
    public void RecordTrigger()
    {
        LastTriggeredAt = DateTime.UtcNow;
        TriggerCount++;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the action configuration.
    /// </summary>
    public void UpdateActionConfig(string actionConfig)
    {
        ActionConfig = actionConfig;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if the rule applies to a specific device.
    /// </summary>
    public bool AppliesTo(string deviceId, string deviceType)
    {
        if (!IsEnabled)
            return false;

        if (!string.IsNullOrWhiteSpace(DeviceIdFilter) && DeviceIdFilter != deviceId)
            return false;

        if (!string.IsNullOrWhiteSpace(DeviceTypeFilter) && DeviceTypeFilter != deviceType)
            return false;

        return true;
    }
}
