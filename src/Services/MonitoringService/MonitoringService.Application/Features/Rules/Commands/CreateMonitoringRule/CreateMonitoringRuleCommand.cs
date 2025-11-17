using FluentResults;
using FluentValidation;
using MediatR;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Domain.Entities;
using MonitoringService.Domain.Enums;

namespace MonitoringService.Application.Features.Rules.Commands.CreateMonitoringRule;

/// <summary>
/// Command to create a new monitoring rule.
/// </summary>
public record CreateMonitoringRuleCommand(
    string Name,
    string Description,
    RuleConditionType ConditionType,
    string ConditionValue,
    RuleActionType ActionType,
    Domain.Enums.Severity AlertSeverity,
    string? DeviceIdFilter = null,
    string? DeviceTypeFilter = null) : IRequest<Result<string>>;

/// <summary>
/// Validator for CreateMonitoringRuleCommand.
/// </summary>
public class CreateMonitoringRuleCommandValidator : AbstractValidator<CreateMonitoringRuleCommand>
{
    public CreateMonitoringRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rule name is required")
            .MaximumLength(100).WithMessage("Rule name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ConditionType)
            .IsInEnum().WithMessage("Invalid condition type");

        RuleFor(x => x.ConditionValue)
            .NotEmpty().WithMessage("Condition value is required");

        RuleFor(x => x.ActionType)
            .IsInEnum().WithMessage("Invalid action type");

        RuleFor(x => x.AlertSeverity)
            .IsInEnum().WithMessage("Invalid alert severity");

        RuleFor(x => x.DeviceIdFilter)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.DeviceIdFilter))
            .WithMessage("Device ID filter must not exceed 100 characters");

        RuleFor(x => x.DeviceTypeFilter)
            .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.DeviceTypeFilter))
            .WithMessage("Device type filter must not exceed 50 characters");
    }
}

/// <summary>
/// Handler for CreateMonitoringRuleCommand.
/// </summary>
public class CreateMonitoringRuleCommandHandler : IRequestHandler<CreateMonitoringRuleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public CreateMonitoringRuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateMonitoringRuleCommand request, CancellationToken cancellationToken)
    {
        // Create new monitoring rule
        var rule = MonitoringRule.Create(
            request.Name,
            request.Description,
            request.ConditionType,
            request.ConditionValue,
            request.ActionType,
            request.AlertSeverity,
            request.DeviceIdFilter,
            request.DeviceTypeFilter);

        _context.MonitoringRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(rule.Id);
    }
}
