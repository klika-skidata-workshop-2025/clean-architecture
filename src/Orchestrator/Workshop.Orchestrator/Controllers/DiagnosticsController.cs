using Microsoft.AspNetCore.Mvc;
using Workshop.Orchestrator.Models;
using Workshop.Orchestrator.Services;

namespace Workshop.Orchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticsOrchestrator _diagnosticsOrchestrator;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IDiagnosticsOrchestrator diagnosticsOrchestrator,
        ILogger<DiagnosticsController> logger)
    {
        _diagnosticsOrchestrator = diagnosticsOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Gets system health across all services
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(SystemHealthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemHealthResponse>> GetSystemHealth(CancellationToken cancellationToken)
    {
        try
        {
            var health = await _diagnosticsOrchestrator.GetSystemHealthAsync(cancellationToken);
            return Ok(health);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get system health");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets error logs
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(List<ErrorLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ErrorLogResponse>>> GetErrorLogs([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _diagnosticsOrchestrator.GetErrorLogsAsync(limit, cancellationToken);
            return Ok(logs);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get error logs");
            return BadRequest(new { error = ex.Message });
        }
    }
}
