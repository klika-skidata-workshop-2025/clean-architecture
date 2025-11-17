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
using Workshop.Proto.Monitoring;

namespace MonitoringService.API.Services;

/// <summary>
/// gRPC service implementation for Monitoring Service.
/// Maps proto definitions to CQRS commands and queries.
/// </summary>
public class MonitoringGrpcService : MonitoringService.MonitoringServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MonitoringGrpcService> _logger;

    public MonitoringGrpcService(IMediator mediator, ILogger<MonitoringGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<GetActiveAlertsResponse> GetActiveAlerts(
        GetActiveAlertsRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetActiveAlerts called");

        var query = new GetActiveAlertsQuery(
            MinimumSeverity: request.MinimumSeverity != Workshop.Proto.Common.Severity.Unspecified
                ? MapSeverityToDomain(request.MinimumSeverity)
                : null,
            DeviceId: !string.IsNullOrWhiteSpace(request.DeviceId) ? request.DeviceId : null,
            PageNumber: request.PageNumber > 0 ? request.PageNumber : 1,
            PageSize: request.PageSize > 0 ? request.PageSize : 20);

        var result = await _mediator.Send(query, context.CancellationToken);

        return await result.MatchAsync(
            dto =>
            {
                var response = new GetActiveAlertsResponse
                {
                    TotalCount = dto.TotalCount,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize,
                    TotalPages = dto.TotalPages
                };

                response.Alerts.AddRange(dto.Alerts.Select(a => new AlertInfo
                {
                    AlertId = a.Id,
                    Title = a.Title,
                    Message = a.Message,
                    Severity = MapSeverity(a.Severity),
                    Status = MapAlertStatus(a.Status),
                    DeviceId = a.DeviceId ?? string.Empty,
                    DeviceName = a.DeviceName ?? string.Empty,
                    RuleId = a.RuleId ?? string.Empty,
                    RuleName = a.RuleName ?? string.Empty,
                    AcknowledgedBy = a.AcknowledgedBy ?? string.Empty,
                    AcknowledgedAt = a.AcknowledgedAt.HasValue
                        ? Timestamp.FromDateTime(a.AcknowledgedAt.Value.ToUniversalTime())
                        : null,
                    CreatedAt = Timestamp.FromDateTime(a.CreatedAt.ToUniversalTime())
                }));

                return Task.FromResult(response);
            },
            error => throw error.ToRpcException()
        );
    }

    public override async Task<GetAlertHistoryResponse> GetAlertHistory(
        GetAlertHistoryRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetAlertHistory called");

        var query = new GetAlertHistoryQuery(
            StartTime: request.StartTime?.ToDateTime(),
            EndTime: request.EndTime?.ToDateTime(),
            Status: request.Status != Workshop.Proto.Monitoring.AlertStatus.Unspecified
                ? MapAlertStatusToDomain(request.Status)
                : null,
            MinimumSeverity: request.MinimumSeverity != Workshop.Proto.Common.Severity.Unspecified
                ? MapSeverityToDomain(request.MinimumSeverity)
                : null,
            DeviceId: !string.IsNullOrWhiteSpace(request.DeviceId) ? request.DeviceId : null,
            PageNumber: request.PageNumber > 0 ? request.PageNumber : 1,
            PageSize: request.PageSize > 0 ? request.PageSize : 20);

        var result = await _mediator.Send(query, context.CancellationToken);

        return await result.MatchAsync(
            dto =>
            {
                var response = new GetAlertHistoryResponse
                {
                    TotalCount = dto.TotalCount,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize,
                    TotalPages = dto.TotalPages
                };

                response.Alerts.AddRange(dto.Alerts.Select(a => new AlertInfo
                {
                    AlertId = a.Id,
                    Title = a.Title,
                    Message = a.Message,
                    Severity = MapSeverity(a.Severity),
                    Status = MapAlertStatus(a.Status),
                    DeviceId = a.DeviceId ?? string.Empty,
                    DeviceName = a.DeviceName ?? string.Empty,
                    RuleId = a.RuleId ?? string.Empty,
                    RuleName = a.RuleName ?? string.Empty,
                    AcknowledgedBy = a.AcknowledgedBy ?? string.Empty,
                    AcknowledgedAt = a.AcknowledgedAt.HasValue
                        ? Timestamp.FromDateTime(a.AcknowledgedAt.Value.ToUniversalTime())
                        : null,
                    CreatedAt = Timestamp.FromDateTime(a.CreatedAt.ToUniversalTime())
                }));

                return Task.FromResult(response);
            },
            error => throw error.ToRpcException()
        );
    }

    public override async Task<GetAlertResponse> GetAlert(
        GetAlertRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetAlert called for alert: {AlertId}", request.AlertId);

        var query = new GetAlertQuery(request.AlertId);
        var result = await _mediator.Send(query, context.CancellationToken);

        return await result.MatchAsync(
            a => Task.FromResult(new GetAlertResponse
            {
                AlertId = a.Id,
                Title = a.Title,
                Message = a.Message,
                Severity = MapSeverity(a.Severity),
                Status = MapAlertStatus(a.Status),
                DeviceId = a.DeviceId ?? string.Empty,
                DeviceName = a.DeviceName ?? string.Empty,
                RuleId = a.RuleId ?? string.Empty,
                RuleName = a.RuleName ?? string.Empty,
                AcknowledgedBy = a.AcknowledgedBy ?? string.Empty,
                AcknowledgedAt = a.AcknowledgedAt.HasValue
                    ? Timestamp.FromDateTime(a.AcknowledgedAt.Value.ToUniversalTime())
                    : null,
                CreatedAt = Timestamp.FromDateTime(a.CreatedAt.ToUniversalTime()),
                UpdatedAt = Timestamp.FromDateTime(a.UpdatedAt.ToUniversalTime())
            }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<AcknowledgeAlertResponse> AcknowledgeAlert(
        AcknowledgeAlertRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("AcknowledgeAlert called for alert: {AlertId}", request.AlertId);

        var command = new AcknowledgeAlertCommand(request.AlertId, request.AcknowledgedBy);
        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            () => Task.FromResult(new AcknowledgeAlertResponse { Success = true }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<ListMonitoringRulesResponse> ListMonitoringRules(
        ListMonitoringRulesRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("ListMonitoringRules called");

        var query = new ListMonitoringRulesQuery(
            IsEnabled: request.HasIsEnabled ? request.IsEnabled : null,
            PageNumber: request.PageNumber > 0 ? request.PageNumber : 1,
            PageSize: request.PageSize > 0 ? request.PageSize : 20);

        var result = await _mediator.Send(query, context.CancellationToken);

        return await result.MatchAsync(
            dto =>
            {
                var response = new ListMonitoringRulesResponse
                {
                    TotalCount = dto.TotalCount,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize,
                    TotalPages = dto.TotalPages
                };

                response.Rules.AddRange(dto.Rules.Select(r => new MonitoringRuleInfo
                {
                    RuleId = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsEnabled = r.IsEnabled,
                    ConditionType = MapRuleConditionType(r.ConditionType),
                    ConditionValue = r.ConditionValue,
                    ActionType = MapRuleActionType(r.ActionType),
                    AlertSeverity = MapSeverity(r.AlertSeverity),
                    DeviceIdFilter = r.DeviceIdFilter ?? string.Empty,
                    DeviceTypeFilter = r.DeviceTypeFilter ?? string.Empty,
                    LastTriggeredAt = r.LastTriggeredAt.HasValue
                        ? Timestamp.FromDateTime(r.LastTriggeredAt.Value.ToUniversalTime())
                        : null,
                    TriggerCount = r.TriggerCount
                }));

                return Task.FromResult(response);
            },
            error => throw error.ToRpcException()
        );
    }

    public override async Task<CreateMonitoringRuleResponse> CreateMonitoringRule(
        CreateMonitoringRuleRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("CreateMonitoringRule called: {RuleName}", request.Name);

        var command = new CreateMonitoringRuleCommand(
            request.Name,
            request.Description,
            MapRuleConditionTypeToDomain(request.ConditionType),
            request.ConditionValue,
            MapRuleActionTypeToDomain(request.ActionType),
            MapSeverityToDomain(request.AlertSeverity),
            !string.IsNullOrWhiteSpace(request.DeviceIdFilter) ? request.DeviceIdFilter : null,
            !string.IsNullOrWhiteSpace(request.DeviceTypeFilter) ? request.DeviceTypeFilter : null);

        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            ruleId => Task.FromResult(new CreateMonitoringRuleResponse
            {
                RuleId = ruleId,
                Success = true
            }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<UpdateMonitoringRuleResponse> UpdateMonitoringRule(
        UpdateMonitoringRuleRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("UpdateMonitoringRule called for rule: {RuleId}", request.RuleId);

        var command = new UpdateMonitoringRuleCommand(
            request.RuleId,
            !string.IsNullOrWhiteSpace(request.Name) ? request.Name : null,
            !string.IsNullOrWhiteSpace(request.Description) ? request.Description : null,
            request.ConditionType != Workshop.Proto.Monitoring.RuleConditionType.Unspecified
                ? MapRuleConditionTypeToDomain(request.ConditionType)
                : null,
            !string.IsNullOrWhiteSpace(request.ConditionValue) ? request.ConditionValue : null,
            request.ActionType != Workshop.Proto.Monitoring.RuleActionType.Unspecified
                ? MapRuleActionTypeToDomain(request.ActionType)
                : null,
            request.AlertSeverity != Workshop.Proto.Common.Severity.Unspecified
                ? MapSeverityToDomain(request.AlertSeverity)
                : null,
            request.HasDeviceIdFilter ? request.DeviceIdFilter : null,
            request.HasDeviceTypeFilter ? request.DeviceTypeFilter : null,
            request.HasIsEnabled ? request.IsEnabled : null);

        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            () => Task.FromResult(new UpdateMonitoringRuleResponse { Success = true }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<DeleteMonitoringRuleResponse> DeleteMonitoringRule(
        DeleteMonitoringRuleRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("DeleteMonitoringRule called for rule: {RuleId}", request.RuleId);

        var command = new DeleteMonitoringRuleCommand(request.RuleId);
        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            () => Task.FromResult(new DeleteMonitoringRuleResponse { Success = true }),
            error => throw error.ToRpcException()
        );
    }

    // Helper methods to map between proto and domain enums

    private static Workshop.Proto.Common.Severity MapSeverity(Domain.Enums.Severity severity)
    {
        return severity switch
        {
            Domain.Enums.Severity.Info => Workshop.Proto.Common.Severity.Info,
            Domain.Enums.Severity.Warning => Workshop.Proto.Common.Severity.Warning,
            Domain.Enums.Severity.Error => Workshop.Proto.Common.Severity.Error,
            Domain.Enums.Severity.Critical => Workshop.Proto.Common.Severity.Critical,
            _ => Workshop.Proto.Common.Severity.Unspecified
        };
    }

    private static Domain.Enums.Severity MapSeverityToDomain(Workshop.Proto.Common.Severity severity)
    {
        return severity switch
        {
            Workshop.Proto.Common.Severity.Info => Domain.Enums.Severity.Info,
            Workshop.Proto.Common.Severity.Warning => Domain.Enums.Severity.Warning,
            Workshop.Proto.Common.Severity.Error => Domain.Enums.Severity.Error,
            Workshop.Proto.Common.Severity.Critical => Domain.Enums.Severity.Critical,
            _ => Domain.Enums.Severity.Info
        };
    }

    private static Workshop.Proto.Monitoring.AlertStatus MapAlertStatus(Domain.Enums.AlertStatus status)
    {
        return status switch
        {
            Domain.Enums.AlertStatus.Active => Workshop.Proto.Monitoring.AlertStatus.Active,
            Domain.Enums.AlertStatus.Acknowledged => Workshop.Proto.Monitoring.AlertStatus.Acknowledged,
            Domain.Enums.AlertStatus.Resolved => Workshop.Proto.Monitoring.AlertStatus.Resolved,
            _ => Workshop.Proto.Monitoring.AlertStatus.Unspecified
        };
    }

    private static Domain.Enums.AlertStatus MapAlertStatusToDomain(Workshop.Proto.Monitoring.AlertStatus status)
    {
        return status switch
        {
            Workshop.Proto.Monitoring.AlertStatus.Active => Domain.Enums.AlertStatus.Active,
            Workshop.Proto.Monitoring.AlertStatus.Acknowledged => Domain.Enums.AlertStatus.Acknowledged,
            Workshop.Proto.Monitoring.AlertStatus.Resolved => Domain.Enums.AlertStatus.Resolved,
            _ => Domain.Enums.AlertStatus.Active
        };
    }

    private static Workshop.Proto.Monitoring.RuleConditionType MapRuleConditionType(Domain.Enums.RuleConditionType type)
    {
        return type switch
        {
            Domain.Enums.RuleConditionType.DeviceStatusEquals => Workshop.Proto.Monitoring.RuleConditionType.DeviceStatusEquals,
            Domain.Enums.RuleConditionType.DeviceOfflineForDuration => Workshop.Proto.Monitoring.RuleConditionType.DeviceOfflineForDuration,
            Domain.Enums.RuleConditionType.DeviceEventTypeMatches => Workshop.Proto.Monitoring.RuleConditionType.DeviceEventTypeMatches,
            Domain.Enums.RuleConditionType.CustomExpression => Workshop.Proto.Monitoring.RuleConditionType.CustomExpression,
            _ => Workshop.Proto.Monitoring.RuleConditionType.Unspecified
        };
    }

    private static Domain.Enums.RuleConditionType MapRuleConditionTypeToDomain(Workshop.Proto.Monitoring.RuleConditionType type)
    {
        return type switch
        {
            Workshop.Proto.Monitoring.RuleConditionType.DeviceStatusEquals => Domain.Enums.RuleConditionType.DeviceStatusEquals,
            Workshop.Proto.Monitoring.RuleConditionType.DeviceOfflineForDuration => Domain.Enums.RuleConditionType.DeviceOfflineForDuration,
            Workshop.Proto.Monitoring.RuleConditionType.DeviceEventTypeMatches => Domain.Enums.RuleConditionType.DeviceEventTypeMatches,
            Workshop.Proto.Monitoring.RuleConditionType.CustomExpression => Domain.Enums.RuleConditionType.CustomExpression,
            _ => Domain.Enums.RuleConditionType.DeviceStatusEquals
        };
    }

    private static Workshop.Proto.Monitoring.RuleActionType MapRuleActionType(Domain.Enums.RuleActionType type)
    {
        return type switch
        {
            Domain.Enums.RuleActionType.CreateAlert => Workshop.Proto.Monitoring.RuleActionType.CreateAlert,
            Domain.Enums.RuleActionType.SendNotification => Workshop.Proto.Monitoring.RuleActionType.SendNotification,
            Domain.Enums.RuleActionType.ExecuteCustomAction => Workshop.Proto.Monitoring.RuleActionType.ExecuteCustomAction,
            _ => Workshop.Proto.Monitoring.RuleActionType.Unspecified
        };
    }

    private static Domain.Enums.RuleActionType MapRuleActionTypeToDomain(Workshop.Proto.Monitoring.RuleActionType type)
    {
        return type switch
        {
            Workshop.Proto.Monitoring.RuleActionType.CreateAlert => Domain.Enums.RuleActionType.CreateAlert,
            Workshop.Proto.Monitoring.RuleActionType.SendNotification => Domain.Enums.RuleActionType.SendNotification,
            Workshop.Proto.Monitoring.RuleActionType.ExecuteCustomAction => Domain.Enums.RuleActionType.ExecuteCustomAction,
            _ => Domain.Enums.RuleActionType.CreateAlert
        };
    }
}
