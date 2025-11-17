using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Application.Features.Alerts.Queries.GetActiveAlerts;
using MonitoringService.Domain.Enums;

namespace MonitoringService.Application.Features.Alerts.Queries.GetAlertHistory;

/// <summary>
/// Query to get alert history (all alerts including acknowledged and resolved).
/// </summary>
public record GetAlertHistoryQuery(
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    AlertStatus? Status = null,
    Domain.Enums.Severity? MinimumSeverity = null,
    string? DeviceId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<GetAlertHistoryResponse>>;

/// <summary>
/// Response containing alert history with pagination.
/// </summary>
public record GetAlertHistoryResponse(
    List<AlertDto> Alerts,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Validator for GetAlertHistoryQuery.
/// </summary>
public class GetAlertHistoryQueryValidator : AbstractValidator<GetAlertHistoryQuery>
{
    public GetAlertHistoryQueryValidator()
    {
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("End time must be after start time");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Invalid alert status");

        RuleFor(x => x.MinimumSeverity)
            .IsInEnum().When(x => x.MinimumSeverity.HasValue)
            .WithMessage("Invalid severity");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
    }
}

/// <summary>
/// Handler for GetAlertHistoryQuery.
/// </summary>
public class GetAlertHistoryQueryHandler : IRequestHandler<GetAlertHistoryQuery, Result<GetAlertHistoryResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAlertHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetAlertHistoryResponse>> Handle(GetAlertHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Alerts.AsQueryable();

        // Apply filters
        if (request.StartTime.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= request.StartTime.Value);
        }

        if (request.EndTime.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= request.EndTime.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (request.MinimumSeverity.HasValue)
        {
            query = query.Where(a => a.Severity >= request.MinimumSeverity.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.DeviceId))
        {
            query = query.Where(a => a.DeviceId == request.DeviceId);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Apply pagination and ordering
        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AlertDto(
                a.Id,
                a.Title,
                a.Message,
                a.Severity,
                a.Status,
                a.DeviceId,
                a.DeviceName,
                a.RuleId,
                a.RuleName,
                a.AcknowledgedAt,
                a.AcknowledgedBy,
                a.CreatedAt,
                a.UpdatedAt))
            .ToListAsync(cancellationToken);

        var response = new GetAlertHistoryResponse(
            alerts,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Ok(response);
    }
}
