using DiagnosticsService.Domain.Common;
using DiagnosticsService.Domain.Enums;

namespace DiagnosticsService.Domain.Entities;

/// <summary>
/// Represents a health snapshot of a service at a specific point in time.
/// </summary>
public class HealthSnapshot : BaseEntity
{
    public string ServiceName { get; private set; } = string.Empty;
    public HealthStatus Status { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public long ResponseTimeMs { get; private set; }
    public string Dependencies { get; private set; } = "{}";
    public string Metrics { get; private set; } = "{}";

    private HealthSnapshot() { }

    public static HealthSnapshot Create(
        string serviceName,
        HealthStatus status,
        string description,
        long responseTimeMs = 0)
    {
        return new HealthSnapshot
        {
            ServiceName = serviceName,
            Status = status,
            Description = description,
            ResponseTimeMs = responseTimeMs
        };
    }

    public void UpdateDependencies(string dependencies)
    {
        Dependencies = dependencies;
        MarkAsUpdated();
    }

    public void UpdateMetrics(string metrics)
    {
        Metrics = metrics;
        MarkAsUpdated();
    }
}
