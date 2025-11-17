using DiagnosticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ErrorLog> ErrorLogs { get; }
    DbSet<HealthSnapshot> HealthSnapshots { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
