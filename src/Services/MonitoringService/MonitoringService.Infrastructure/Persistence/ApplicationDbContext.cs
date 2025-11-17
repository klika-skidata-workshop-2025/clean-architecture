using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Domain.Entities;

namespace MonitoringService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for MonitoringService.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Alerts entity set.
    /// </summary>
    public DbSet<Alert> Alerts => Set<Alert>();

    /// <summary>
    /// MonitoringRules entity set.
    /// </summary>
    public DbSet<MonitoringRule> MonitoringRules => Set<MonitoringRule>();

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

        // Alert entity configuration
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.RuleId);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.DeviceId)
                .HasMaxLength(100);

            entity.Property(e => e.DeviceName)
                .HasMaxLength(100);

            entity.Property(e => e.RuleId)
                .HasMaxLength(100);

            entity.Property(e => e.RuleName)
                .HasMaxLength(100);

            entity.Property(e => e.AcknowledgedBy)
                .HasMaxLength(100);

            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
        });

        // MonitoringRule entity configuration
        modelBuilder.Entity<MonitoringRule>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.ConditionType);
            entity.HasIndex(e => e.DeviceIdFilter);
            entity.HasIndex(e => e.DeviceTypeFilter);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ConditionValue)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.ActionConfig)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");

            entity.Property(e => e.DeviceIdFilter)
                .HasMaxLength(100);

            entity.Property(e => e.DeviceTypeFilter)
                .HasMaxLength(50);
        });
    }
}
