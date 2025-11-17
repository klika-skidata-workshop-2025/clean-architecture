using Grpc.Core;
using Workshop.Contracts.Monitoring;
using Workshop.Orchestrator.Models;
using GrpcCreateMonitoringRuleRequest = Workshop.Contracts.Monitoring.CreateMonitoringRuleRequest;
using GrpcAlertInfo = Workshop.Contracts.Monitoring.AlertInfo;
using GrpcMonitoringRuleInfo = Workshop.Contracts.Monitoring.MonitoringRuleInfo;

namespace Workshop.Orchestrator.Services;

public class MonitoringOrchestrator : IMonitoringOrchestrator
{
    private readonly MonitoringService.MonitoringServiceClient _monitoringClient;
    private readonly ILogger<MonitoringOrchestrator> _logger;

    public MonitoringOrchestrator(
        MonitoringService.MonitoringServiceClient monitoringClient,
        ILogger<MonitoringOrchestrator> logger)
    {
        _monitoringClient = monitoringClient;
        _logger = logger;
    }

    public async Task<List<AlertResponse>> GetActiveAlertsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetActiveAlertsRequest();
            var response = await _monitoringClient.GetActiveAlertsAsync(request, cancellationToken: cancellationToken);

            return response.Alerts.Select(MapToAlertResponse).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get active alerts");
            throw new InvalidOperationException($"Failed to get active alerts: {ex.Status.Detail}", ex);
        }
    }

    public async Task<List<AlertResponse>> GetAlertHistoryAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetAlertHistoryRequest { DeviceId = deviceId };
            var response = await _monitoringClient.GetAlertHistoryAsync(request, cancellationToken: cancellationToken);

            return response.Alerts.Select(MapToAlertResponse).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get alert history for device {DeviceId}", deviceId);
            throw new InvalidOperationException($"Failed to get alert history: {ex.Status.Detail}", ex);
        }
    }

    public async Task AcknowledgeAlertAsync(string alertId, string acknowledgedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AcknowledgeAlertRequest
            {
                AlertId = alertId,
                AcknowledgedBy = acknowledgedBy
            };

            await _monitoringClient.AcknowledgeAlertAsync(request, cancellationToken: cancellationToken);

            _logger.LogInformation("Acknowledged alert {AlertId} by {User}", alertId, acknowledgedBy);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to acknowledge alert {AlertId}", alertId);
            throw new InvalidOperationException($"Failed to acknowledge alert: {ex.Status.Detail}", ex);
        }
    }

    public async Task<MonitoringRuleResponse> CreateMonitoringRuleAsync(Models.CreateMonitoringRuleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcRequest = new GrpcCreateMonitoringRuleRequest
            {
                RuleName = request.RuleName,
                Description = $"Rule created via API: {request.RuleName}",
                ConditionType = Enum.Parse<Workshop.Contracts.Monitoring.RuleConditionType>(request.Condition, true),
                Severity = Enum.Parse<Workshop.Contracts.Common.Severity>(request.Severity, true),
                Enabled = true
            };

            // Add action types
            var actionType = Enum.Parse<Workshop.Contracts.Monitoring.RuleActionType>(request.Action, true);
            grpcRequest.Actions.Add(actionType);

            // Add filter configuration if provided
            if (!string.IsNullOrEmpty(request.DeviceIdFilter))
            {
                grpcRequest.Configuration.Add("device_id_filter", request.DeviceIdFilter);
            }
            if (!string.IsNullOrEmpty(request.DeviceTypeFilter))
            {
                grpcRequest.Configuration.Add("device_type_filter", request.DeviceTypeFilter);
            }

            var response = await _monitoringClient.CreateMonitoringRuleAsync(grpcRequest, cancellationToken: cancellationToken);

            return MapToMonitoringRuleResponse(response.Rule);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to create monitoring rule {RuleName}", request.RuleName);
            throw new InvalidOperationException($"Failed to create monitoring rule: {ex.Status.Detail}", ex);
        }
    }

    public async Task<List<MonitoringRuleResponse>> ListMonitoringRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListMonitoringRulesRequest();
            var response = await _monitoringClient.ListMonitoringRulesAsync(request, cancellationToken: cancellationToken);

            return response.Rules.Select(MapToMonitoringRuleResponse).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to list monitoring rules");
            throw new InvalidOperationException($"Failed to list monitoring rules: {ex.Status.Detail}", ex);
        }
    }

    private static AlertResponse MapToAlertResponse(GrpcAlertInfo alert)
    {
        return new AlertResponse(
            alert.AlertId,
            alert.DeviceId,
            alert.Severity.ToString(),
            alert.Status.ToString(),
            alert.Description, // Using description as message
            alert.TriggeredAt.ToDateTime(),
            alert.AcknowledgedAt?.ToDateTime(),
            alert.AcknowledgedBy);
    }

    private static MonitoringRuleResponse MapToMonitoringRuleResponse(GrpcMonitoringRuleInfo rule)
    {
        // Extract filters from configuration
        rule.Configuration.TryGetValue("device_id_filter", out var deviceIdFilter);
        rule.Configuration.TryGetValue("device_type_filter", out var deviceTypeFilter);

        // Get first action as string (simplified - real implementation would handle multiple actions)
        var actionString = rule.Actions.Count > 0 ? rule.Actions[0].ToString() : "None";

        return new MonitoringRuleResponse(
            rule.RuleId,
            rule.RuleName,
            rule.ConditionType.ToString(),
            actionString,
            rule.Severity.ToString(),
            rule.Enabled,
            deviceIdFilter,
            deviceTypeFilter,
            rule.TimesTriggered);
    }
}
