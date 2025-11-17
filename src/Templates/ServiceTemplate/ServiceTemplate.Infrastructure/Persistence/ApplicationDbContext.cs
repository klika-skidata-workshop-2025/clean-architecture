using Microsoft.EntityFrameworkCore;
using ServiceTemplate.Application.Common.Interfaces;

namespace ServiceTemplate.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the service.
/// Implements IApplicationDbContext for use in the Application layer.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // TODO: Add DbSet properties for your entities here
    // Example:
    // public DbSet<YourEntity> YourEntities => Set<YourEntity>();

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps for all tracked entities
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                // Use reflection to call protected MarkAsUpdated method
                var markAsUpdatedMethod = entry.Entity.GetType()
                    .GetMethod("MarkAsUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                markAsUpdatedMethod?.Invoke(entry.Entity, null);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // TODO: Add global query filters, indexes, and other configurations here
    }
}
