using System.Diagnostics;
using Workshop.Orchestrator.Models;

namespace Workshop.Orchestrator.Services;

public class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly IDeviceOrchestrator _deviceOrchestrator;
    private readonly IMonitoringOrchestrator _monitoringOrchestrator;
    private readonly ILogger<WorkflowOrchestrator> _logger;

    public WorkflowOrchestrator(
        IDeviceOrchestrator deviceOrchestrator,
        IMonitoringOrchestrator monitoringOrchestrator,
        ILogger<WorkflowOrchestrator> logger)
    {
        _deviceOrchestrator = deviceOrchestrator;
        _monitoringOrchestrator = monitoringOrchestrator;
        _logger = logger;
    }

    public async Task<WorkflowResult> ExecuteCompleteWorkflowAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var steps = new List<WorkflowStep>();
        var overallSuccess = true;

        try
        {
            // Step 1: Register a new device
            var step1 = await ExecuteStepAsync(1, "Register Device", async () =>
            {
                var request = new RegisterDeviceRequest(
                    deviceId,
                    "GATE",
                    "Main Entrance",
                    "{\"zone\": \"A\"}");

                var device = await _deviceOrchestrator.RegisterDeviceAsync(request, cancellationToken);
                return $"Registered device {device.DeviceId} of type {device.DeviceType}";
            });
            steps.Add(step1);
            overallSuccess &= step1.Success;

            // Step 2: Create monitoring rule for the device
            var step2 = await ExecuteStepAsync(2, "Create Monitoring Rule", async () =>
            {
                var request = new CreateMonitoringRuleRequest(
                    $"Monitor {deviceId} Status",
                    "DEVICE_OFFLINE",
                    "CREATE_ALERT",
                    "CRITICAL",
                    deviceId,
                    null);

                var rule = await _monitoringOrchestrator.CreateMonitoringRuleAsync(request, cancellationToken);
                return $"Created monitoring rule {rule.RuleId} for device {deviceId}";
            });
            steps.Add(step2);
            overallSuccess &= step2.Success;

            // Step 3: Wait a moment for rule to be active
            await Task.Delay(1000, cancellationToken);

            // Step 4: Simulate device going offline
            var step3 = await ExecuteStepAsync(3, "Simulate Device Offline", async () =>
            {
                await _deviceOrchestrator.SimulateDeviceEventAsync(deviceId, "OFFLINE", cancellationToken);
                return $"Simulated {deviceId} going offline";
            });
            steps.Add(step3);
            overallSuccess &= step3.Success;

            // Step 5: Wait for event processing
            await Task.Delay(2000, cancellationToken);

            // Step 6: Verify alert was triggered
            var alertId = string.Empty;
            var step4 = await ExecuteStepAsync(4, "Verify Alert Triggered", async () =>
            {
                var alerts = await _monitoringOrchestrator.GetActiveAlertsAsync(cancellationToken);
                var deviceAlert = alerts.FirstOrDefault(a => a.DeviceId == deviceId);

                if (deviceAlert == null)
                {
                    throw new InvalidOperationException($"No alert found for device {deviceId}");
                }

                alertId = deviceAlert.AlertId;
                return $"Alert {alertId} triggered with severity {deviceAlert.Severity}";
            });
            steps.Add(step4);
            overallSuccess &= step4.Success;

            // Step 7: Acknowledge the alert
            var step5 = await ExecuteStepAsync(5, "Acknowledge Alert", async () =>
            {
                if (string.IsNullOrEmpty(alertId))
                {
                    throw new InvalidOperationException("No alert ID available to acknowledge");
                }

                await _monitoringOrchestrator.AcknowledgeAlertAsync(alertId, "Workshop Orchestrator", cancellationToken);
                return $"Acknowledged alert {alertId}";
            });
            steps.Add(step5);
            overallSuccess &= step5.Success;

            var message = overallSuccess
                ? $"Complete workflow executed successfully for device {deviceId}"
                : $"Workflow completed with errors for device {deviceId}";

            return new WorkflowResult(overallSuccess, message, steps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow execution failed for device {DeviceId}", deviceId);

            return new WorkflowResult(
                false,
                $"Workflow failed: {ex.Message}",
                steps);
        }
    }

    public async Task<DeviceHealthReport> GetDeviceHealthReportAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all devices
            var devices = await _deviceOrchestrator.ListDevicesAsync(cancellationToken);

            var deviceSummaries = new List<DeviceHealthSummary>();
            var healthyCount = 0;
            var unhealthyCount = 0;
            var offlineCount = 0;

            foreach (var device in devices)
            {
                // Get alerts for this device
                var alerts = await _monitoringOrchestrator.GetActiveAlertsAsync(cancellationToken);
                var deviceAlerts = alerts.Where(a => a.DeviceId == device.DeviceId).ToList();

                var summary = new DeviceHealthSummary(
                    device.DeviceId,
                    device.DeviceType,
                    device.Status,
                    device.IsOnline,
                    deviceAlerts.Count,
                    device.LastHeartbeat);

                deviceSummaries.Add(summary);

                // Count health status
                if (!device.IsOnline)
                {
                    offlineCount++;
                }
                else if (deviceAlerts.Any(a => a.Severity == "CRITICAL" || a.Severity == "HIGH"))
                {
                    unhealthyCount++;
                }
                else
                {
                    healthyCount++;
                }
            }

            return new DeviceHealthReport(
                DateTime.UtcNow,
                devices.Count,
                healthyCount,
                unhealthyCount,
                offlineCount,
                deviceSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate device health report");
            throw;
        }
    }

    private async Task<WorkflowStep> ExecuteStepAsync(int stepNumber, string stepName, Func<Task<string>> action)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await action();
            sw.Stop();

            _logger.LogInformation("Step {StepNumber} ({StepName}) completed: {Result}", stepNumber, stepName, result);

            return new WorkflowStep(stepNumber, stepName, true, result, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(ex, "Step {StepNumber} ({StepName}) failed", stepNumber, stepName);

            return new WorkflowStep(stepNumber, stepName, false, $"Error: {ex.Message}", sw.Elapsed);
        }
    }
}
