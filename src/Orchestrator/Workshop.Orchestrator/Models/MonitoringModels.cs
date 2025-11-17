namespace Workshop.Orchestrator.Models;

public record AlertResponse(
    string AlertId,
    string DeviceId,
    string Severity,
    string Status,
    string Message,
    DateTime TriggeredAt,
    DateTime? AcknowledgedAt,
    string? AcknowledgedBy);

public record CreateMonitoringRuleRequest(
    string RuleName,
    string Condition,
    string Action,
    string Severity,
    string? DeviceIdFilter,
    string? DeviceTypeFilter);

public record MonitoringRuleResponse(
    string RuleId,
    string RuleName,
    string Condition,
    string Action,
    string Severity,
    bool IsEnabled,
    string? DeviceIdFilter,
    string? DeviceTypeFilter,
    int TriggerCount);
