using Microsoft.EntityFrameworkCore;
using MonitoringService.Domain.Entities;

namespace MonitoringService.Application.Common.Interfaces;

/// <summary>
/// Database context interface for the Application layer.
/// Provides access to database entities without depending on EF Core implementation details.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the Alerts entity set.
    /// </summary>
    DbSet<Alert> Alerts { get; }

    /// <summary>
    /// Gets the MonitoringRules entity set.
    /// </summary>
    DbSet<MonitoringRule> MonitoringRules { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
