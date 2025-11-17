using Microsoft.AspNetCore.Mvc;
using Workshop.Orchestrator.Models;
using Workshop.Orchestrator.Services;

namespace Workshop.Orchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(
        IWorkflowOrchestrator workflowOrchestrator,
        ILogger<WorkflowsController> logger)
    {
        _workflowOrchestrator = workflowOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Executes complete workflow demonstration:
    /// 1. Register device
    /// 2. Create monitoring rule
    /// 3. Simulate device offline
    /// 4. Verify alert triggered
    /// 5. Acknowledge alert
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(WorkflowResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowResult>> ExecuteCompleteWorkflow([FromBody] WorkflowRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workflowOrchestrator.ExecuteCompleteWorkflowAsync(request.DeviceId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow execution failed");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets device health report across all devices
    /// </summary>
    [HttpGet("device-health-report")]
    [ProducesResponseType(typeof(DeviceHealthReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeviceHealthReport>> GetDeviceHealthReport(CancellationToken cancellationToken)
    {
        try
        {
            var report = await _workflowOrchestrator.GetDeviceHealthReportAsync(cancellationToken);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate device health report");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record WorkflowRequest(string DeviceId);
