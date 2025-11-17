using Grpc.Core;
using Microsoft.Extensions.Logging;
using Workshop.Proto.Device;

namespace DeviceService.Client;

/// <summary>
/// Client for DeviceService gRPC service.
/// Provides a strongly-typed interface for managing devices.
/// </summary>
public class DeviceServiceClient
{
    private readonly Workshop.Proto.Device.DeviceService.DeviceServiceClient _client;
    private readonly ILogger<DeviceServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of DeviceServiceClient.
    /// </summary>
    public DeviceServiceClient(
        Workshop.Proto.Device.DeviceService.DeviceServiceClient client,
        ILogger<DeviceServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Gets the status of a specific device.
    /// </summary>
    public async Task<GetDeviceStatusResponse> GetDeviceStatusAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting status for device: {DeviceId}", deviceId);

            var request = new GetDeviceStatusRequest { DeviceId = deviceId };
            var response = await _client.GetDeviceStatusAsync(request, cancellationToken: cancellationToken);

            _logger.LogDebug("Device status retrieved: {DeviceId} - {Status}", deviceId, response.Status);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting device status: {StatusCode} - {Detail}",
                ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }

    /// <summary>
    /// Lists devices with optional filtering.
    /// </summary>
    public async Task<ListDevicesResponse> ListDevicesAsync(
        DeviceStatus? status = null,
        DeviceType? type = null,
        string? location = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Listing devices - Status: {Status}, Type: {Type}, Page: {PageNumber}",
                status, type, pageNumber);

            var request = new ListDevicesRequest
            {
                Status = status ?? DeviceStatus.Unspecified,
                Type = type ?? DeviceType.Unspecified,
                Location = location ?? string.Empty,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _client.ListDevicesAsync(request, cancellationToken: cancellationToken);

            _logger.LogDebug("Found {Count} devices", response.TotalCount);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error listing devices: {StatusCode} - {Detail}",
                ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }

    /// <summary>
    /// Registers a new device.
    /// </summary>
    public async Task<RegisterDeviceResponse> RegisterDeviceAsync(
        string name,
        DeviceType type,
        string location,
        string ipAddress,
        string firmwareVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Registering device: {Name} ({Type})", name, type);

            var request = new RegisterDeviceRequest
            {
                Name = name,
                Type = type,
                Location = location,
                IpAddress = ipAddress,
                FirmwareVersion = firmwareVersion
            };

            var response = await _client.RegisterDeviceAsync(request, cancellationToken: cancellationToken);

            _logger.LogInformation("Device registered: {DeviceId}", response.DeviceId);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error registering device: {StatusCode} - {Detail}",
                ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }

    /// <summary>
    /// Updates a device's status and information.
    /// </summary>
    public async Task<UpdateDeviceResponse> UpdateDeviceAsync(
        string deviceId,
        DeviceStatus? status = null,
        string? name = null,
        string? location = null,
        string? firmwareVersion = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating device: {DeviceId}", deviceId);

            var request = new UpdateDeviceRequest
            {
                DeviceId = deviceId,
                Status = status ?? DeviceStatus.Unspecified,
                Name = name ?? string.Empty,
                Location = location ?? string.Empty,
                FirmwareVersion = firmwareVersion ?? string.Empty
            };

            var response = await _client.UpdateDeviceAsync(request, cancellationToken: cancellationToken);

            _logger.LogInformation("Device updated: {DeviceId}", deviceId);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error updating device: {StatusCode} - {Detail}",
                ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }

    /// <summary>
    /// Simulates a device event (for testing/demo purposes).
    /// </summary>
    public async Task<SimulateDeviceEventResponse> SimulateDeviceEventAsync(
        string deviceId,
        DeviceEventType eventType,
        string eventData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Simulating device event: {DeviceId} - {EventType}", deviceId, eventType);

            var request = new SimulateDeviceEventRequest
            {
                DeviceId = deviceId,
                EventType = eventType,
                EventData = eventData
            };

            var response = await _client.SimulateDeviceEventAsync(request, cancellationToken: cancellationToken);

            _logger.LogInformation("Device event simulated: {DeviceId}", deviceId);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error simulating device event: {StatusCode} - {Detail}",
                ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }

    /// <summary>
    /// Gets heartbeat history for a device.
    /// </summary>
    public async Task<GetDeviceHeartbeatsResponse> GetDeviceHeartbeatsAsync(
        string deviceId,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting heartbeats for device: {DeviceId}", deviceId);

            var request = new GetDeviceHeartbeatsRequest
            {
                DeviceId = deviceId
            };

            if (startTime.HasValue)
                request.StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(startTime.Value.ToUniversalTime());

            if (endTime.HasValue)
                request.EndTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(endTime.Value.ToUniversalTime());

            var response = await _client.GetDeviceHeartbeatsAsync(request, cancellationToken: cancellationToken);

            _logger.LogDebug("Heartbeats retrieved for device: {DeviceId}", deviceId);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting device heartbeats: {StatusCode} - {Detail}",
                ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }
}
