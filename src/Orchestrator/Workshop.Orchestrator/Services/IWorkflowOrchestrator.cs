using Workshop.Orchestrator.Models;

namespace Workshop.Orchestrator.Services;

/// <summary>
/// Orchestrates complex workflows across multiple microservices
/// </summary>
public interface IWorkflowOrchestrator
{
    /// <summary>
    /// Demonstrates complete workflow:
    /// 1. Register a new device
    /// 2. Create monitoring rule for the device
    /// 3. Simulate device going offline
    /// 4. Verify alert was triggered
    /// 5. Acknowledge the alert
    /// </summary>
    Task<WorkflowResult> ExecuteCompleteWorkflowAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Demonstrates device health monitoring workflow:
    /// 1. Get all devices
    /// 2. Check each device status
    /// 3. Get alerts for unhealthy devices
    /// </summary>
    Task<DeviceHealthReport> GetDeviceHealthReportAsync(CancellationToken cancellationToken = default);
}
