using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using Workshop.Common.Errors;

namespace MonitoringService.Application.Features.Alerts.Commands.AcknowledgeAlert;

/// <summary>
/// Command to acknowledge an alert.
/// </summary>
public record AcknowledgeAlertCommand(
    string AlertId,
    string AcknowledgedBy) : IRequest<Result>;

/// <summary>
/// Validator for AcknowledgeAlertCommand.
/// </summary>
public class AcknowledgeAlertCommandValidator : AbstractValidator<AcknowledgeAlertCommand>
{
    public AcknowledgeAlertCommandValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("Alert ID is required");

        RuleFor(x => x.AcknowledgedBy)
            .NotEmpty().WithMessage("AcknowledgedBy is required")
            .MaximumLength(100).WithMessage("AcknowledgedBy must not exceed 100 characters");
    }
}

/// <summary>
/// Handler for AcknowledgeAlertCommand.
/// </summary>
public class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AcknowledgeAlertCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        // Find alert
        var alert = await _context.Alerts
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null)
        {
            return CommonErrors.Monitoring.AlertNotFound(request.AlertId);
        }

        // Check if already acknowledged
        if (alert.Status == Domain.Enums.AlertStatus.Acknowledged)
        {
            return CommonErrors.Monitoring.AlertAlreadyAcknowledged(request.AlertId);
        }

        // Acknowledge the alert
        alert.Acknowledge(request.AcknowledgedBy);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
