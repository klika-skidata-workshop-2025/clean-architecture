using Microsoft.EntityFrameworkCore;

namespace ServiceTemplate.Application.Common.Interfaces;

/// <summary>
/// Interface for the application's database context.
/// This abstraction allows the Application layer to interact with the database
/// without depending on Entity Framework Core directly.
/// </summary>
/// <remarks>
/// The Infrastructure layer implements this interface with Entity Framework Core.
/// This allows us to mock the database in unit tests.
/// </remarks>
public interface IApplicationDbContext
{
    // Add DbSet properties for your entities here
    // Example:
    // DbSet<Device> Devices { get; }

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
