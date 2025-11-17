using DiagnosticsService.Application.Common.Interfaces;
using DiagnosticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiagnosticsService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
    public DbSet<HealthSnapshot> HealthSnapshots => Set<HealthSnapshot>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
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

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Metadata).HasColumnType("jsonb").HasDefaultValue("{}");
        });

        modelBuilder.Entity<HealthSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Dependencies).HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.Metrics).HasColumnType("jsonb").HasDefaultValue("{}");
        });
    }
}
