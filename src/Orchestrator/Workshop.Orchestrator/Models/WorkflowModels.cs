namespace Workshop.Orchestrator.Models;

public record WorkflowResult(
    bool Success,
    string Message,
    List<WorkflowStep> Steps);

public record WorkflowStep(
    int StepNumber,
    string StepName,
    bool Success,
    string Result,
    TimeSpan Duration);

public record DeviceHealthReport(
    DateTime GeneratedAt,
    int TotalDevices,
    int HealthyDevices,
    int UnhealthyDevices,
    int OfflineDevices,
    List<DeviceHealthSummary> DeviceSummaries);

public record DeviceHealthSummary(
    string DeviceId,
    string DeviceType,
    string Status,
    bool IsOnline,
    int ActiveAlertCount,
    DateTime? LastHeartbeat);
