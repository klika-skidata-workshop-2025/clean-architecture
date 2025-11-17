using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Application.Features.Alerts.Queries.GetActiveAlerts;
using Workshop.Common.Errors;

namespace MonitoringService.Application.Features.Alerts.Queries.GetAlert;

/// <summary>
/// Query to get a single alert by ID.
/// </summary>
public record GetAlertQuery(string AlertId) : IRequest<Result<AlertDto>>;

/// <summary>
/// Validator for GetAlertQuery.
/// </summary>
public class GetAlertQueryValidator : AbstractValidator<GetAlertQuery>
{
    public GetAlertQueryValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("Alert ID is required");
    }
}

/// <summary>
/// Handler for GetAlertQuery.
/// </summary>
public class GetAlertQueryHandler : IRequestHandler<GetAlertQuery, Result<AlertDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlertQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AlertDto>> Handle(GetAlertQuery request, CancellationToken cancellationToken)
    {
        var alert = await _context.Alerts
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null)
        {
            return CommonErrors.Monitoring.AlertNotFound(request.AlertId);
        }

        var dto = new AlertDto(
            alert.Id,
            alert.Title,
            alert.Message,
            alert.Severity,
            alert.Status,
            alert.DeviceId,
            alert.DeviceName,
            alert.RuleId,
            alert.RuleName,
            alert.AcknowledgedAt,
            alert.AcknowledgedBy,
            alert.CreatedAt,
            alert.UpdatedAt);

        return Result.Ok(dto);
    }
}
