using Microsoft.AspNetCore.Mvc;
using Workshop.Orchestrator.Models;
using Workshop.Orchestrator.Services;

namespace Workshop.Orchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringOrchestrator _monitoringOrchestrator;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IMonitoringOrchestrator monitoringOrchestrator,
        ILogger<MonitoringController> logger)
    {
        _monitoringOrchestrator = monitoringOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Gets active alerts
    /// </summary>
    [HttpGet("alerts/active")]
    [ProducesResponseType(typeof(List<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertResponse>>> GetActiveAlerts(CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _monitoringOrchestrator.GetActiveAlertsAsync(cancellationToken);
            return Ok(alerts);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get active alerts");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets alert history for a device
    /// </summary>
    [HttpGet("alerts/history/{deviceId}")]
    [ProducesResponseType(typeof(List<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertResponse>>> GetAlertHistory(string deviceId, CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _monitoringOrchestrator.GetAlertHistoryAsync(deviceId, cancellationToken);
            return Ok(alerts);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get alert history");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Acknowledges an alert
    /// </summary>
    [HttpPost("alerts/{alertId}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcknowledgeAlert(string alertId, [FromBody] AcknowledgeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _monitoringOrchestrator.AcknowledgeAlertAsync(alertId, request.AcknowledgedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to acknowledge alert");
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a monitoring rule
    /// </summary>
    [HttpPost("rules")]
    [ProducesResponseType(typeof(MonitoringRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MonitoringRuleResponse>> CreateMonitoringRule([FromBody] CreateMonitoringRuleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _monitoringOrchestrator.CreateMonitoringRuleAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ListMonitoringRules), new { ruleId = rule.RuleId }, rule);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create monitoring rule");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lists all monitoring rules
    /// </summary>
    [HttpGet("rules")]
    [ProducesResponseType(typeof(List<MonitoringRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MonitoringRuleResponse>>> ListMonitoringRules(CancellationToken cancellationToken)
    {
        try
        {
            var rules = await _monitoringOrchestrator.ListMonitoringRulesAsync(cancellationToken);
            return Ok(rules);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to list monitoring rules");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record AcknowledgeRequest(string AcknowledgedBy);
