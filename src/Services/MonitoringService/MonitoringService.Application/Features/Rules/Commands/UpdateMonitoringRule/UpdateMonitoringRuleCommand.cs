using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Domain.Enums;
using Workshop.Common.Errors;

namespace MonitoringService.Application.Features.Rules.Commands.UpdateMonitoringRule;

/// <summary>
/// Command to update an existing monitoring rule.
/// </summary>
public record UpdateMonitoringRuleCommand(
    string RuleId,
    string? Name = null,
    string? Description = null,
    RuleConditionType? ConditionType = null,
    string? ConditionValue = null,
    RuleActionType? ActionType = null,
    Domain.Enums.Severity? AlertSeverity = null,
    string? DeviceIdFilter = null,
    string? DeviceTypeFilter = null,
    bool? IsEnabled = null) : IRequest<Result>;

/// <summary>
/// Validator for UpdateMonitoringRuleCommand.
/// </summary>
public class UpdateMonitoringRuleCommandValidator : AbstractValidator<UpdateMonitoringRuleCommand>
{
    public UpdateMonitoringRuleCommandValidator()
    {
        RuleFor(x => x.RuleId)
            .NotEmpty().WithMessage("Rule ID is required");

        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Rule name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ConditionType)
            .IsInEnum().When(x => x.ConditionType.HasValue)
            .WithMessage("Invalid condition type");

        RuleFor(x => x.ActionType)
            .IsInEnum().When(x => x.ActionType.HasValue)
            .WithMessage("Invalid action type");

        RuleFor(x => x.AlertSeverity)
            .IsInEnum().When(x => x.AlertSeverity.HasValue)
            .WithMessage("Invalid alert severity");
    }
}

/// <summary>
/// Handler for UpdateMonitoringRuleCommand.
/// </summary>
public class UpdateMonitoringRuleCommandHandler : IRequestHandler<UpdateMonitoringRuleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateMonitoringRuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateMonitoringRuleCommand request, CancellationToken cancellationToken)
    {
        // Find rule
        var rule = await _context.MonitoringRules
            .FirstOrDefaultAsync(r => r.Id == request.RuleId, cancellationToken);

        if (rule == null)
        {
            return CommonErrors.Monitoring.RuleNotFound(request.RuleId);
        }

        // Update rule
        rule.Update(
            request.Name,
            request.Description,
            request.ConditionType,
            request.ConditionValue,
            request.ActionType,
            request.AlertSeverity,
            request.DeviceIdFilter,
            request.DeviceTypeFilter);

        // Update enabled status if specified
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value)
                rule.Enable();
            else
                rule.Disable();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
