using DiagnosticsService.Application.Common.Interfaces;
using DiagnosticsService.Domain.Enums;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsService.Application.Features.ErrorLogs.Queries.GetErrorLogs;

public record GetErrorLogsQuery(
    string? ServiceName = null,
    LogLevel? MinimumLevel = null,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<GetErrorLogsResponse>>;

public record ErrorLogDto(
    string Id,
    string ServiceName,
    LogLevel Level,
    string Message,
    string? StackTrace,
    string? Source,
    DateTime CreatedAt);

public record GetErrorLogsResponse(
    List<ErrorLogDto> ErrorLogs,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

public class GetErrorLogsQueryValidator : AbstractValidator<GetErrorLogsQuery>
{
    public GetErrorLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class GetErrorLogsQueryHandler : IRequestHandler<GetErrorLogsQuery, Result<GetErrorLogsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetErrorLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetErrorLogsResponse>> Handle(GetErrorLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ErrorLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ServiceName))
            query = query.Where(e => e.ServiceName == request.ServiceName);

        if (request.MinimumLevel.HasValue)
            query = query.Where(e => e.Level >= request.MinimumLevel.Value);

        if (request.StartTime.HasValue)
            query = query.Where(e => e.CreatedAt >= request.StartTime.Value);

        if (request.EndTime.HasValue)
            query = query.Where(e => e.CreatedAt <= request.EndTime.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var errorLogs = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new ErrorLogDto(
                e.Id,
                e.ServiceName,
                e.Level,
                e.Message,
                e.StackTrace,
                e.Source,
                e.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Ok(new GetErrorLogsResponse(
            errorLogs,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages));
    }
}
