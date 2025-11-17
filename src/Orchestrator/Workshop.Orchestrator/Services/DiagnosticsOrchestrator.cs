using Grpc.Core;
using Workshop.Contracts.Diagnostics;
using Workshop.Orchestrator.Models;

namespace Workshop.Orchestrator.Services;

public class DiagnosticsOrchestrator : IDiagnosticsOrchestrator
{
    private readonly DiagnosticsService.DiagnosticsServiceClient _diagnosticsClient;
    private readonly ILogger<DiagnosticsOrchestrator> _logger;

    public DiagnosticsOrchestrator(
        DiagnosticsService.DiagnosticsServiceClient diagnosticsClient,
        ILogger<DiagnosticsOrchestrator> logger)
    {
        _diagnosticsClient = diagnosticsClient;
        _logger = logger;
    }

    public async Task<SystemHealthResponse> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetSystemHealthRequest();
            var response = await _diagnosticsClient.GetSystemHealthAsync(request, cancellationToken: cancellationToken);

            var services = response.Services.Select(s => new ServiceHealthResponse(
                s.ServiceName,
                s.Status.ToString(),
                (long)s.ResponseTimeMs,
                s.ErrorMessage ?? string.Empty)).ToList();

            return new SystemHealthResponse(
                response.OverallStatus.ToString(),
                response.CheckedAt.ToDateTime(),
                services);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get system health");
            throw new InvalidOperationException($"Failed to get system health: {ex.Status.Detail}", ex);
        }
    }

    public async Task<List<ErrorLogResponse>> GetErrorLogsAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetErrorLogsRequest();
            // Pagination is handled via the pagination field, not a separate limit
            var response = await _diagnosticsClient.GetErrorLogsAsync(request, cancellationToken: cancellationToken);

            return response.Logs.Select(log => new ErrorLogResponse(
                log.LogId,
                log.ServiceName,
                log.Level.ToString(),
                log.Message,
                log.Exception?.StackTrace ?? string.Empty,
                log.Timestamp.ToDateTime())).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get error logs");
            throw new InvalidOperationException($"Failed to get error logs: {ex.Status.Detail}", ex);
        }
    }
}
