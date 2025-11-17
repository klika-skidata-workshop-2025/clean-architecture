using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Domain.Enums;

namespace MonitoringService.Application.Features.Rules.Queries.ListMonitoringRules;

/// <summary>
/// Query to list all monitoring rules with optional filtering.
/// </summary>
public record ListMonitoringRulesQuery(
    bool? IsEnabled = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<ListMonitoringRulesResponse>>;

/// <summary>
/// Monitoring rule DTO.
/// </summary>
public record MonitoringRuleDto(
    string Id,
    string Name,
    string Description,
    bool IsEnabled,
    RuleConditionType ConditionType,
    string ConditionValue,
    RuleActionType ActionType,
    Severity AlertSeverity,
    string? DeviceIdFilter,
    string? DeviceTypeFilter,
    DateTime? LastTriggeredAt,
    int TriggerCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Response containing monitoring rules with pagination.
/// </summary>
public record ListMonitoringRulesResponse(
    List<MonitoringRuleDto> Rules,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Validator for ListMonitoringRulesQuery.
/// </summary>
public class ListMonitoringRulesQueryValidator : AbstractValidator<ListMonitoringRulesQuery>
{
    public ListMonitoringRulesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
    }
}

/// <summary>
/// Handler for ListMonitoringRulesQuery.
/// </summary>
public class ListMonitoringRulesQueryHandler : IRequestHandler<ListMonitoringRulesQuery, Result<ListMonitoringRulesResponse>>
{
    private readonly IApplicationDbContext _context;

    public ListMonitoringRulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ListMonitoringRulesResponse>> Handle(ListMonitoringRulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MonitoringRules.AsQueryable();

        // Apply filters
        if (request.IsEnabled.HasValue)
        {
            query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Apply pagination and ordering
        var rules = await query
            .OrderBy(r => r.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new MonitoringRuleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsEnabled,
                r.ConditionType,
                r.ConditionValue,
                r.ActionType,
                r.AlertSeverity,
                r.DeviceIdFilter,
                r.DeviceTypeFilter,
                r.LastTriggeredAt,
                r.TriggerCount,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        var response = new ListMonitoringRulesResponse(
            rules,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Ok(response);
    }
}
