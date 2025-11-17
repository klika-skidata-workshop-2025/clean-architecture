using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Domain.Enums;

namespace MonitoringService.Application.Features.Alerts.Queries.GetActiveAlerts;

/// <summary>
/// Query to get active (non-acknowledged) alerts with optional filtering.
/// </summary>
public record GetActiveAlertsQuery(
    Domain.Enums.Severity? MinimumSeverity = null,
    string? DeviceId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<GetActiveAlertsResponse>>;

/// <summary>
/// Alert DTO.
/// </summary>
public record AlertDto(
    string Id,
    string Title,
    string Message,
    Domain.Enums.Severity Severity,
    AlertStatus Status,
    string? DeviceId,
    string? DeviceName,
    string? RuleId,
    string? RuleName,
    DateTime? AcknowledgedAt,
    string? AcknowledgedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Response containing active alerts with pagination.
/// </summary>
public record GetActiveAlertsResponse(
    List<AlertDto> Alerts,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Validator for GetActiveAlertsQuery.
/// </summary>
public class GetActiveAlertsQueryValidator : AbstractValidator<GetActiveAlertsQuery>
{
    public GetActiveAlertsQueryValidator()
    {
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
/// Handler for GetActiveAlertsQuery.
/// </summary>
public class GetActiveAlertsQueryHandler : IRequestHandler<GetActiveAlertsQuery, Result<GetActiveAlertsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveAlertsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetActiveAlertsResponse>> Handle(GetActiveAlertsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Alerts
            .Where(a => a.Status == AlertStatus.Active);

        // Apply filters
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

        var response = new GetActiveAlertsResponse(
            alerts,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Ok(response);
    }
}
