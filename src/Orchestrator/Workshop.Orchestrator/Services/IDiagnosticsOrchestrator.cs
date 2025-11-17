using Workshop.Orchestrator.Models;

namespace Workshop.Orchestrator.Services;

/// <summary>
/// Orchestrates diagnostics-related operations across microservices
/// </summary>
public interface IDiagnosticsOrchestrator
{
    /// <summary>
    /// Gets system health across all services
    /// </summary>
    Task<SystemHealthResponse> GetSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets error logs
    /// </summary>
    Task<List<ErrorLogResponse>> GetErrorLogsAsync(int limit = 50, CancellationToken cancellationToken = default);
}
