using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using MonitoringService.Application.Features.Alerts.Commands.AcknowledgeAlert;
using MonitoringService.Application.Features.Alerts.Queries.GetActiveAlerts;
using MonitoringService.Application.Features.Alerts.Queries.GetAlert;
using MonitoringService.Application.Features.Alerts.Queries.GetAlertHistory;
using MonitoringService.Application.Features.Rules.Commands.CreateMonitoringRule;
using MonitoringService.Application.Features.Rules.Commands.DeleteMonitoringRule;
using MonitoringService.Application.Features.Rules.Commands.UpdateMonitoringRule;
using MonitoringService.Application.Features.Rules.Queries.ListMonitoringRules;
using Workshop.Common.Extensions;
using Workshop.Contracts.Monitoring;
using Workshop.Contracts.Common;
using GrpcMonitoringService = Workshop.Contracts.Monitoring.MonitoringService;

namespace MonitoringService.API.Services;

public class MonitoringGrpcService : GrpcMonitoringService.MonitoringServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MonitoringGrpcService> _logger;

    public MonitoringGrpcService(IMediator mediator, ILogger<MonitoringGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<Workshop.Contracts.Monitoring.GetActiveAlertsResponse> GetActiveAlerts(
        Workshop.Contracts.Monitoring.GetActiveAlertsRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetActiveAlerts called");

        var pageNumber = request.Pagination?.Page ?? 0;
        var pageSize = request.Pagination?.PageSize ?? 20;

        var query = new GetActiveAlertsQuery(
            MinimumSeverity: request.MinSeverity != Severity.Unspecified ? MapSeverityToDomain(request.MinSeverity) : null,
            DeviceId: !string.IsNullOrWhiteSpace(request.DeviceId) ? request.DeviceId : null,
            PageNumber: pageNumber > 0 ? pageNumber : 1,
            PageSize: pageSize > 0 ? pageSize : 20);

        return await _mediator.Send(query, context.CancellationToken).MatchAsync(
            dto =>
            {
                var response = new Workshop.Contracts.Monitoring.GetActiveAlertsResponse
                {
                    Pagination = new PaginationResponse
                    {
                        CurrentPage = dto.PageNumber, PageSize = dto.PageSize, TotalItems = dto.TotalCount,
                        TotalPages = dto.TotalPages, HasNext = dto.PageNumber < dto.TotalPages, HasPrevious = dto.PageNumber > 1
                    }
                };
                response.Alerts.AddRange(dto.Alerts.Select(MapAlertToProto));
                return response;
            },
            error => error.ToRpcException());
    }

    public override async Task<Workshop.Contracts.Monitoring.GetAlertHistoryResponse> GetAlertHistory(
        Workshop.Contracts.Monitoring.GetAlertHistoryRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetAlertHistory called");

        var pageNumber = request.Pagination?.Page ?? 0;
        var pageSize = request.Pagination?.PageSize ?? 20;

        var query = new GetAlertHistoryQuery(
            StartTime: request.TimeRange?.Start?.ToDateTime(),
            EndTime: request.TimeRange?.End?.ToDateTime(),
            Status: request.Status != AlertStatus.Unspecified ? MapAlertStatusToDomain(request.Status) : null,
            MinimumSeverity: null,
            DeviceId: !string.IsNullOrWhiteSpace(request.DeviceId) ? request.DeviceId : null,
            PageNumber: pageNumber > 0 ? pageNumber : 1,
            PageSize: pageSize > 0 ? pageSize : 20);

        return await _mediator.Send(query, context.CancellationToken).MatchAsync(
            dto =>
            {
                var response = new Workshop.Contracts.Monitoring.GetAlertHistoryResponse
                {
                    Pagination = new PaginationResponse
                    {
                        CurrentPage = dto.PageNumber, PageSize = dto.PageSize, TotalItems = dto.TotalCount,
                        TotalPages = dto.TotalPages, HasNext = dto.PageNumber < dto.TotalPages, HasPrevious = dto.PageNumber > 1
                    }
                };
                response.Alerts.AddRange(dto.Alerts.Select(MapAlertToProto));
                return response;
            },
            error => error.ToRpcException());
    }

    public override async Task<GetAlertResponse> GetAlert(GetAlertRequest request, ServerCallContext context)
    {
        _logger.LogDebug("GetAlert called for alert: {AlertId}", request.AlertId);

        return await _mediator.Send(new GetAlertQuery(request.AlertId), context.CancellationToken).MatchAsync(
            a => new GetAlertResponse { Alert = MapAlertToProto(a) },
            error => error.ToRpcException());
    }

    public override async Task<AcknowledgeAlertResponse> AcknowledgeAlert(AcknowledgeAlertRequest request, ServerCallContext context)
    {
        _logger.LogDebug("AcknowledgeAlert called for alert: {AlertId}", request.AlertId);

        await _mediator.Send(new AcknowledgeAlertCommand(request.AlertId, request.AcknowledgedBy), context.CancellationToken).ThrowIfFailureAsync();

        return new AcknowledgeAlertResponse { Success = true, Message = "Alert acknowledged successfully" };
    }

    public override async Task<Workshop.Contracts.Monitoring.ListMonitoringRulesResponse> ListMonitoringRules(
        Workshop.Contracts.Monitoring.ListMonitoringRulesRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("ListMonitoringRules called");

        var pageNumber = request.Pagination?.Page ?? 0;
        var pageSize = request.Pagination?.PageSize ?? 20;

        var query = new ListMonitoringRulesQuery(
            IsEnabled: request.HasEnabled ? request.Enabled : null,
            PageNumber: pageNumber > 0 ? pageNumber : 1,
            PageSize: pageSize > 0 ? pageSize : 20);

        return await _mediator.Send(query, context.CancellationToken).MatchAsync(
            dto =>
            {
                var response = new Workshop.Contracts.Monitoring.ListMonitoringRulesResponse
                {
                    Pagination = new PaginationResponse
                    {
                        CurrentPage = dto.PageNumber, PageSize = dto.PageSize, TotalItems = dto.TotalCount,
                        TotalPages = dto.TotalPages, HasNext = dto.PageNumber < dto.TotalPages, HasPrevious = dto.PageNumber > 1
                    }
                };
                response.Rules.AddRange(dto.Rules.Select(MapRuleToProto));
                return response;
            },
            error => error.ToRpcException());
    }

    public override async Task<CreateMonitoringRuleResponse> CreateMonitoringRule(CreateMonitoringRuleRequest request, ServerCallContext context)
    {
        _logger.LogDebug("CreateMonitoringRule called: {RuleName}", request.RuleName);

        var conditionValue = request.Configuration.TryGetValue("condition_value", out var cv) ? cv : string.Empty;
        var deviceIdFilter = request.Configuration.TryGetValue("device_id_filter", out var didf) ? didf : null;
        var deviceTypeFilter = request.Configuration.TryGetValue("device_type_filter", out var dtf) ? dtf : null;
        var actionType = request.Actions.Count > 0 ? MapRuleActionTypeToDomain(request.Actions[0]) : Domain.Enums.RuleActionType.CreateAlert;

        var command = new CreateMonitoringRuleCommand(
            request.RuleName, request.Description, MapRuleConditionTypeToDomain(request.ConditionType),
            conditionValue, actionType, MapSeverityToDomain(request.Severity), deviceIdFilter, deviceTypeFilter);

        return await _mediator.Send(command, context.CancellationToken).MatchAsync(
            ruleId => new CreateMonitoringRuleResponse
            {
                Success = true,
                Message = "Rule created successfully",
                Rule = new MonitoringRuleInfo { RuleId = ruleId, RuleName = request.RuleName, Description = request.Description, ConditionType = request.ConditionType, Severity = request.Severity, Enabled = request.Enabled }
            },
            error => error.ToRpcException());
    }

    public override async Task<UpdateMonitoringRuleResponse> UpdateMonitoringRule(UpdateMonitoringRuleRequest request, ServerCallContext context)
    {
        _logger.LogDebug("UpdateMonitoringRule called for rule: {RuleId}", request.RuleId);

        var conditionValue = request.Configuration.TryGetValue("condition_value", out var cv) ? cv : null;
        var deviceIdFilter = request.Configuration.TryGetValue("device_id_filter", out var didf) ? didf : null;
        var deviceTypeFilter = request.Configuration.TryGetValue("device_type_filter", out var dtf) ? dtf : null;
        Domain.Enums.RuleActionType? actionType = request.Actions.Count > 0 ? MapRuleActionTypeToDomain(request.Actions[0]) : null;

        var command = new UpdateMonitoringRuleCommand(
            request.RuleId,
            request.HasRuleName ? request.RuleName : null,
            request.HasDescription ? request.Description : null,
            null, conditionValue, actionType,
            request.HasSeverity ? MapSeverityToDomain(request.Severity) : null,
            deviceIdFilter, deviceTypeFilter,
            request.HasEnabled ? request.Enabled : null);

        await _mediator.Send(command, context.CancellationToken).ThrowIfFailureAsync();

        return new UpdateMonitoringRuleResponse { Success = true, Message = "Rule updated successfully" };
    }

    public override async Task<DeleteMonitoringRuleResponse> DeleteMonitoringRule(DeleteMonitoringRuleRequest request, ServerCallContext context)
    {
        _logger.LogDebug("DeleteMonitoringRule called for rule: {RuleId}", request.RuleId);

        await _mediator.Send(new DeleteMonitoringRuleCommand(request.RuleId), context.CancellationToken).ThrowIfFailureAsync();

        return new DeleteMonitoringRuleResponse { Success = true, Message = "Rule deleted successfully" };
    }

    private AlertInfo MapAlertToProto(Application.Features.Alerts.Queries.GetActiveAlerts.AlertDto a)
    {
        var alertInfo = new AlertInfo
        {
            AlertId = a.Id, Title = a.Title, Description = a.Message, Severity = MapSeverity(a.Severity),
            Status = MapAlertStatus(a.Status), DeviceId = a.DeviceId ?? string.Empty, DeviceName = a.DeviceName ?? string.Empty,
            RuleId = a.RuleId ?? string.Empty, RuleName = a.RuleName ?? string.Empty,
            TriggeredAt = Timestamp.FromDateTime(a.CreatedAt.ToUniversalTime())
        };
        if (!string.IsNullOrEmpty(a.AcknowledgedBy)) alertInfo.AcknowledgedBy = a.AcknowledgedBy;
        if (a.AcknowledgedAt.HasValue) alertInfo.AcknowledgedAt = Timestamp.FromDateTime(a.AcknowledgedAt.Value.ToUniversalTime());
        return alertInfo;
    }

    private MonitoringRuleInfo MapRuleToProto(Application.Features.Rules.Queries.ListMonitoringRules.MonitoringRuleDto r)
    {
        var ruleInfo = new MonitoringRuleInfo
        {
            RuleId = r.Id, RuleName = r.Name, Description = r.Description, Enabled = r.IsEnabled,
            ConditionType = MapRuleConditionType(r.ConditionType), Severity = MapSeverity(r.AlertSeverity),
            TimesTriggered = r.TriggerCount, CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow), UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        ruleInfo.Configuration.Add("condition_value", r.ConditionValue);
        if (!string.IsNullOrEmpty(r.DeviceIdFilter)) ruleInfo.Configuration.Add("device_id_filter", r.DeviceIdFilter);
        if (!string.IsNullOrEmpty(r.DeviceTypeFilter)) ruleInfo.Configuration.Add("device_type_filter", r.DeviceTypeFilter);
        ruleInfo.Actions.Add(MapRuleActionType(r.ActionType));
        if (r.LastTriggeredAt.HasValue) ruleInfo.LastTriggered = Timestamp.FromDateTime(r.LastTriggeredAt.Value.ToUniversalTime());
        return ruleInfo;
    }

    private static Severity MapSeverity(Domain.Enums.Severity s) => s switch
    {
        Domain.Enums.Severity.Info => Severity.Info, Domain.Enums.Severity.Warning => Severity.Warning,
        Domain.Enums.Severity.Error => Severity.Error, Domain.Enums.Severity.Critical => Severity.Critical, _ => Severity.Unspecified
    };

    private static Domain.Enums.Severity MapSeverityToDomain(Severity s) => s switch
    {
        Severity.Info => Domain.Enums.Severity.Info, Severity.Warning => Domain.Enums.Severity.Warning,
        Severity.Error => Domain.Enums.Severity.Error, Severity.Critical => Domain.Enums.Severity.Critical, _ => Domain.Enums.Severity.Info
    };

    private static AlertStatus MapAlertStatus(Domain.Enums.AlertStatus s) => s switch
    {
        Domain.Enums.AlertStatus.Active => AlertStatus.Active, Domain.Enums.AlertStatus.Acknowledged => AlertStatus.Acknowledged,
        Domain.Enums.AlertStatus.Resolved => AlertStatus.Resolved, _ => AlertStatus.Unspecified
    };

    private static Domain.Enums.AlertStatus MapAlertStatusToDomain(AlertStatus s) => s switch
    {
        AlertStatus.Active => Domain.Enums.AlertStatus.Active, AlertStatus.Acknowledged => Domain.Enums.AlertStatus.Acknowledged,
        AlertStatus.Resolved => Domain.Enums.AlertStatus.Resolved, AlertStatus.AutoResolved => Domain.Enums.AlertStatus.Resolved, _ => Domain.Enums.AlertStatus.Active
    };

    private static RuleConditionType MapRuleConditionType(Domain.Enums.RuleConditionType t) => t switch
    {
        Domain.Enums.RuleConditionType.DeviceStatusEquals => RuleConditionType.StatusEquals, Domain.Enums.RuleConditionType.DeviceOfflineForDuration => RuleConditionType.OfflineDuration,
        Domain.Enums.RuleConditionType.DeviceEventTypeMatches => RuleConditionType.EventOccurred, Domain.Enums.RuleConditionType.CustomExpression => RuleConditionType.StatusChanged, _ => RuleConditionType.Unspecified
    };

    private static Domain.Enums.RuleConditionType MapRuleConditionTypeToDomain(RuleConditionType t) => t switch
    {
        RuleConditionType.StatusEquals => Domain.Enums.RuleConditionType.DeviceStatusEquals, RuleConditionType.StatusChanged => Domain.Enums.RuleConditionType.CustomExpression,
        RuleConditionType.OfflineDuration => Domain.Enums.RuleConditionType.DeviceOfflineForDuration, RuleConditionType.EventOccurred => Domain.Enums.RuleConditionType.DeviceEventTypeMatches,
        RuleConditionType.HeartbeatMissed => Domain.Enums.RuleConditionType.DeviceOfflineForDuration, _ => Domain.Enums.RuleConditionType.DeviceStatusEquals
    };

    private static RuleActionType MapRuleActionType(Domain.Enums.RuleActionType t) => t switch
    {
        Domain.Enums.RuleActionType.CreateAlert => RuleActionType.CreateAlert, Domain.Enums.RuleActionType.SendNotification => RuleActionType.SendNotification,
        Domain.Enums.RuleActionType.ExecuteCustomAction => RuleActionType.CallDeviceService, _ => RuleActionType.Unspecified
    };

    private static Domain.Enums.RuleActionType MapRuleActionTypeToDomain(RuleActionType t) => t switch
    {
        RuleActionType.CreateAlert => Domain.Enums.RuleActionType.CreateAlert, RuleActionType.SendNotification => Domain.Enums.RuleActionType.SendNotification,
        RuleActionType.CallDeviceService => Domain.Enums.RuleActionType.ExecuteCustomAction, RuleActionType.Escalate => Domain.Enums.RuleActionType.CreateAlert, _ => Domain.Enums.RuleActionType.CreateAlert
    };
}
