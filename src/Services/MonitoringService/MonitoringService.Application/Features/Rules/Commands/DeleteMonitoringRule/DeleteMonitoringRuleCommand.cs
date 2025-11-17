using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using Workshop.Common.Errors;

namespace MonitoringService.Application.Features.Rules.Commands.DeleteMonitoringRule;

/// <summary>
/// Command to delete a monitoring rule.
/// </summary>
public record DeleteMonitoringRuleCommand(string RuleId) : IRequest<Result>;

/// <summary>
/// Validator for DeleteMonitoringRuleCommand.
/// </summary>
public class DeleteMonitoringRuleCommandValidator : AbstractValidator<DeleteMonitoringRuleCommand>
{
    public DeleteMonitoringRuleCommandValidator()
    {
        RuleFor(x => x.RuleId)
            .NotEmpty().WithMessage("Rule ID is required");
    }
}

/// <summary>
/// Handler for DeleteMonitoringRuleCommand.
/// </summary>
public class DeleteMonitoringRuleCommandHandler : IRequestHandler<DeleteMonitoringRuleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteMonitoringRuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteMonitoringRuleCommand request, CancellationToken cancellationToken)
    {
        // Find rule
        var rule = await _context.MonitoringRules
            .FirstOrDefaultAsync(r => r.Id == request.RuleId, cancellationToken);

        if (rule == null)
        {
            return CommonErrors.Monitoring.RuleNotFound(request.RuleId);
        }

        // Delete the rule
        _context.MonitoringRules.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
