using DiagnosticsService.Application.Common.Interfaces;
using DiagnosticsService.Domain.Enums;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsService.Application.Features.Health.Queries.GetSystemHealth;

public record GetSystemHealthQuery() : IRequest<Result<SystemHealthResponse>>;

public record ServiceHealthInfo(
    string ServiceName,
    HealthStatus Status,
    string Description,
    long ResponseTimeMs,
    DateTime LastCheckTime);

public record SystemHealthResponse(
    HealthStatus OverallStatus,
    List<ServiceHealthInfo> Services,
    DateTime Timestamp);

public class GetSystemHealthQueryHandler : IRequestHandler<GetSystemHealthQuery, Result<SystemHealthResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetSystemHealthQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SystemHealthResponse>> Handle(GetSystemHealthQuery request, CancellationToken cancellationToken)
    {
        var latestSnapshots = await _context.HealthSnapshots
            .GroupBy(h => h.ServiceName)
            .Select(g => g.OrderByDescending(h => h.CreatedAt).FirstOrDefault())
            .Where(h => h != null)
            .ToListAsync(cancellationToken);

        var services = latestSnapshots.Select(s => new ServiceHealthInfo(
            s!.ServiceName,
            s.Status,
            s.Description,
            s.ResponseTimeMs,
            s.CreatedAt)).ToList();

        var overallStatus = services.Any()
            ? services.Max(s => s.Status)
            : HealthStatus.Healthy;

        return Result.Ok(new SystemHealthResponse(
            overallStatus,
            services,
            DateTime.UtcNow));
    }
}
