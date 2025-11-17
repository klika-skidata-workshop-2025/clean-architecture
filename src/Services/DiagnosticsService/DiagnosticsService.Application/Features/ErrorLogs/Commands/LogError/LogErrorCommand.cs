using DiagnosticsService.Application.Common.Interfaces;
using DiagnosticsService.Domain.Entities;
using DiagnosticsService.Domain.Enums;
using FluentResults;
using FluentValidation;
using MediatR;

namespace DiagnosticsService.Application.Features.ErrorLogs.Commands.LogError;

public record LogErrorCommand(
    string ServiceName,
    LogLevel Level,
    string Message,
    string? StackTrace = null,
    string? Source = null) : IRequest<Result<string>>;

public class LogErrorCommandValidator : AbstractValidator<LogErrorCommand>
{
    public LogErrorCommandValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Level).IsInEnum();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public LogErrorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(LogErrorCommand request, CancellationToken cancellationToken)
    {
        var errorLog = ErrorLog.Create(
            request.ServiceName,
            request.Level,
            request.Message,
            request.StackTrace,
            request.Source);

        _context.ErrorLogs.Add(errorLog);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(errorLog.Id);
    }
}
