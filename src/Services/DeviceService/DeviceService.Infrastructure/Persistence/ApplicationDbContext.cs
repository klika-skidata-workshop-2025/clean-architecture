using DeviceService.Application.Common.Interfaces;
using DeviceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for DeviceService.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Devices entity set.
    /// </summary>
    public DbSet<Device> Devices => Set<Device>();

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
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

        // Device entity configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.IpAddress)
                .IsUnique();

            entity.HasIndex(e => e.Status);

            entity.HasIndex(e => e.Type);

            entity.HasIndex(e => e.LastHeartbeat);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasMaxLength(15);

            entity.Property(e => e.FirmwareVersion)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
        });
    }
}
