using Grpc.Core;
using Microsoft.Extensions.Logging;
using Workshop.Proto.Monitoring;

namespace MonitoringService.Client;

/// <summary>
/// Client for MonitoringService gRPC service.
/// </summary>
public class MonitoringServiceClient
{
    private readonly Workshop.Proto.Monitoring.MonitoringService.MonitoringServiceClient _client;
    private readonly ILogger<MonitoringServiceClient> _logger;

    public MonitoringServiceClient(
        Workshop.Proto.Monitoring.MonitoringService.MonitoringServiceClient client,
        ILogger<MonitoringServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<GetActiveAlertsResponse> GetActiveAlertsAsync(
        Workshop.Proto.Common.Severity? minimumSeverity = null,
        string? deviceId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetActiveAlertsRequest
            {
                MinimumSeverity = minimumSeverity ?? Workshop.Proto.Common.Severity.Unspecified,
                DeviceId = deviceId ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await _client.GetActiveAlertsAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting active alerts");
            throw;
        }
    }

    public async Task<AcknowledgeAlertResponse> AcknowledgeAlertAsync(
        string alertId,
        string acknowledgedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AcknowledgeAlertRequest
            {
                AlertId = alertId,
                AcknowledgedBy = acknowledgedBy
            };

            return await _client.AcknowledgeAlertAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error acknowledging alert");
            throw;
        }
    }

    public async Task<ListMonitoringRulesResponse> ListMonitoringRulesAsync(
        bool? isEnabled = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListMonitoringRulesRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            if (isEnabled.HasValue)
                request.IsEnabled = isEnabled.Value;

            return await _client.ListMonitoringRulesAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error listing monitoring rules");
            throw;
        }
    }

    public async Task<CreateMonitoringRuleResponse> CreateMonitoringRuleAsync(
        string name,
        string description,
        RuleConditionType conditionType,
        string conditionValue,
        RuleActionType actionType,
        Workshop.Proto.Common.Severity alertSeverity,
        string? deviceIdFilter = null,
        string? deviceTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateMonitoringRuleRequest
            {
                Name = name,
                Description = description,
                ConditionType = conditionType,
                ConditionValue = conditionValue,
                ActionType = actionType,
                AlertSeverity = alertSeverity,
                DeviceIdFilter = deviceIdFilter ?? string.Empty,
                DeviceTypeFilter = deviceTypeFilter ?? string.Empty
            };

            return await _client.CreateMonitoringRuleAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error creating monitoring rule");
            throw;
        }
    }
}
