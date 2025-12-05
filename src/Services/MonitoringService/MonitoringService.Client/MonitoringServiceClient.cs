using Grpc.Core;
using Microsoft.Extensions.Logging;
using Workshop.Contracts.Monitoring;
using Workshop.Contracts.Common;

namespace MonitoringService.Client;

/// <summary>
/// Client for MonitoringService gRPC service.
/// </summary>
public class MonitoringServiceClient
{
    private readonly Workshop.Contracts.Monitoring.MonitoringService.MonitoringServiceClient _client;
    private readonly ILogger<MonitoringServiceClient> _logger;

    public MonitoringServiceClient(
        Workshop.Contracts.Monitoring.MonitoringService.MonitoringServiceClient client,
        ILogger<MonitoringServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<GetActiveAlertsResponse> GetActiveAlertsAsync(
        Severity? minimumSeverity = null,
        string? deviceId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetActiveAlertsRequest
            {
                MinSeverity = minimumSeverity ?? Severity.Unspecified,
                DeviceId = deviceId ?? string.Empty,
                Pagination = new PaginationRequest
                {
                    Page = pageNumber,
                    PageSize = pageSize
                }
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
                Pagination = new PaginationRequest
                {
                    Page = pageNumber,
                    PageSize = pageSize
                }
            };

            if (isEnabled.HasValue)
                request.Enabled = isEnabled.Value;

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
        Severity alertSeverity,
        string? deviceIdFilter = null,
        string? deviceTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateMonitoringRuleRequest
            {
                RuleName = name,
                Description = description,
                ConditionType = conditionType,
                Severity = alertSeverity,
                Enabled = true
            };

            // Add configuration
            request.Configuration.Add("condition_value", conditionValue);
            if (!string.IsNullOrEmpty(deviceIdFilter))
                request.Configuration.Add("device_id_filter", deviceIdFilter);
            if (!string.IsNullOrEmpty(deviceTypeFilter))
                request.Configuration.Add("device_type_filter", deviceTypeFilter);

            // Add action
            request.Actions.Add(actionType);

            return await _client.CreateMonitoringRuleAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error creating monitoring rule");
            throw;
        }
    }
}
