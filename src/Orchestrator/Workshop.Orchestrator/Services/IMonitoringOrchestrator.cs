using Workshop.Orchestrator.Models;

namespace Workshop.Orchestrator.Services;

/// <summary>
/// Orchestrates monitoring-related operations across microservices
/// </summary>
public interface IMonitoringOrchestrator
{
    /// <summary>
    /// Gets active alerts
    /// </summary>
    Task<List<AlertResponse>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alert history for a device
    /// </summary>
    Task<List<AlertResponse>> GetAlertHistoryAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges an alert
    /// </summary>
    Task AcknowledgeAlertAsync(string alertId, string acknowledgedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a monitoring rule
    /// </summary>
    Task<MonitoringRuleResponse> CreateMonitoringRuleAsync(Models.CreateMonitoringRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all monitoring rules
    /// </summary>
    Task<List<MonitoringRuleResponse>> ListMonitoringRulesAsync(CancellationToken cancellationToken = default);
}
