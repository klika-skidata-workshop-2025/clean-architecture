namespace DiagnosticsService.Domain.Enums;

/// <summary>
/// Health status of a service or component.
/// </summary>
public enum HealthStatus
{
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3
}
