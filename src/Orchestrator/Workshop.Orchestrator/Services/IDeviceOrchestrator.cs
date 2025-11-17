using Workshop.Orchestrator.Models;

namespace Workshop.Orchestrator.Services;

/// <summary>
/// Orchestrates device-related operations across microservices
/// </summary>
public interface IDeviceOrchestrator
{
    /// <summary>
    /// Registers a new device
    /// </summary>
    Task<DeviceResponse> RegisterDeviceAsync(Models.RegisterDeviceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets device status
    /// </summary>
    Task<DeviceResponse> GetDeviceStatusAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all devices
    /// </summary>
    Task<List<DeviceResponse>> ListDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates device status
    /// </summary>
    Task<DeviceResponse> UpdateDeviceAsync(string deviceId, Models.UpdateDeviceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Simulates a device event
    /// </summary>
    Task SimulateDeviceEventAsync(string deviceId, string status, CancellationToken cancellationToken = default);
}
