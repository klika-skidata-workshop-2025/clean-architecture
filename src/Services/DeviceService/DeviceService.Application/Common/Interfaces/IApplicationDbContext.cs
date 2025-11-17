using DeviceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceService.Application.Common.Interfaces;

/// <summary>
/// Database context interface for the Application layer.
/// Provides access to database entities without depending on EF Core implementation details.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the Devices entity set.
    /// </summary>
    DbSet<Device> Devices { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
