using Microsoft.AspNetCore.Mvc;
using Workshop.Orchestrator.Models;
using Workshop.Orchestrator.Services;

namespace Workshop.Orchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceOrchestrator _deviceOrchestrator;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        IDeviceOrchestrator deviceOrchestrator,
        ILogger<DevicesController> logger)
    {
        _deviceOrchestrator = deviceOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new device
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeviceResponse>> RegisterDevice([FromBody] RegisterDeviceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _deviceOrchestrator.RegisterDeviceAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetDeviceStatus), new { deviceId = device.DeviceId }, device);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to register device");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets device status by ID
    /// </summary>
    [HttpGet("{deviceId}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceResponse>> GetDeviceStatus(string deviceId, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _deviceOrchestrator.GetDeviceStatusAsync(deviceId, cancellationToken);
            return Ok(device);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get device status");
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lists all devices
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DeviceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DeviceResponse>>> ListDevices(CancellationToken cancellationToken)
    {
        try
        {
            var devices = await _deviceOrchestrator.ListDevicesAsync(cancellationToken);
            return Ok(devices);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to list devices");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates device status
    /// </summary>
    [HttpPut("{deviceId}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceResponse>> UpdateDevice(string deviceId, [FromBody] UpdateDeviceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _deviceOrchestrator.UpdateDeviceAsync(deviceId, request, cancellationToken);
            return Ok(device);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to update device");
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Simulates a device event
    /// </summary>
    [HttpPost("{deviceId}/simulate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SimulateDeviceEvent(string deviceId, [FromBody] SimulateEventRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _deviceOrchestrator.SimulateDeviceEventAsync(deviceId, request.Status, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to simulate device event");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record SimulateEventRequest(string Status);
