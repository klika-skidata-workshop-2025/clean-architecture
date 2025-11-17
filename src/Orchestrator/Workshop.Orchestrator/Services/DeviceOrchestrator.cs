using Grpc.Core;
using Workshop.Contracts.Device;
using Workshop.Orchestrator.Models;
using GrpcRegisterDeviceRequest = Workshop.Contracts.Device.RegisterDeviceRequest;
using GrpcUpdateDeviceRequest = Workshop.Contracts.Device.UpdateDeviceRequest;
using GrpcDeviceInfo = Workshop.Contracts.Device.DeviceInfo;

namespace Workshop.Orchestrator.Services;

public class DeviceOrchestrator : IDeviceOrchestrator
{
    private readonly DeviceService.DeviceServiceClient _deviceClient;
    private readonly ILogger<DeviceOrchestrator> _logger;

    public DeviceOrchestrator(
        DeviceService.DeviceServiceClient deviceClient,
        ILogger<DeviceOrchestrator> logger)
    {
        _deviceClient = deviceClient;
        _logger = logger;
    }

    public async Task<DeviceResponse> RegisterDeviceAsync(Models.RegisterDeviceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcRequest = new GrpcRegisterDeviceRequest
            {
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceId, // Using DeviceId as default name
                DeviceType = Enum.Parse<Workshop.Contracts.Device.DeviceType>(request.DeviceType, true),
                Location = request.Location
            };

            // Parse metadata if provided (expecting JSON or key=value format)
            if (!string.IsNullOrEmpty(request.Metadata))
            {
                // For simplicity, treat metadata as single key-value pair or skip for now
                // A real implementation would parse JSON or structured data
            }

            var response = await _deviceClient.RegisterDeviceAsync(grpcRequest, cancellationToken: cancellationToken);

            return MapToDeviceResponse(response.Device);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to register device {DeviceId}", request.DeviceId);
            throw new InvalidOperationException($"Failed to register device: {ex.Status.Detail}", ex);
        }
    }

    public async Task<DeviceResponse> GetDeviceStatusAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetDeviceStatusRequest { DeviceId = deviceId };
            var response = await _deviceClient.GetDeviceStatusAsync(request, cancellationToken: cancellationToken);

            return MapToDeviceResponse(response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get device status for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Failed to get device status: {ex.Status.Detail}", ex);
        }
    }

    public async Task<List<DeviceResponse>> ListDevicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListDevicesRequest();
            var response = await _deviceClient.ListDevicesAsync(request, cancellationToken: cancellationToken);

            return response.Devices.Select(MapToDeviceResponse).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to list devices");
            throw new InvalidOperationException($"Failed to list devices: {ex.Status.Detail}", ex);
        }
    }

    public async Task<DeviceResponse> UpdateDeviceAsync(string deviceId, Models.UpdateDeviceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var grpcRequest = new GrpcUpdateDeviceRequest
            {
                DeviceId = deviceId,
                Status = Enum.Parse<DeviceStatus>(request.Status, true),
                Reason = "Updated via Orchestrator API"
            };

            // Parse and add metadata if provided
            if (!string.IsNullOrEmpty(request.Metadata))
            {
                // For simplicity, skip metadata parsing for now
                // A real implementation would parse JSON and populate the map
            }

            var response = await _deviceClient.UpdateDeviceAsync(grpcRequest, cancellationToken: cancellationToken);

            return MapToDeviceResponse(response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to update device {DeviceId}", deviceId);
            throw new InvalidOperationException($"Failed to update device: {ex.Status.Detail}", ex);
        }
    }

    public async Task SimulateDeviceEventAsync(string deviceId, string status, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SimulateDeviceEventRequest
            {
                DeviceId = deviceId,
                EventType = Workshop.Contracts.Device.DeviceEventType.StatusChanged
            };

            await _deviceClient.SimulateDeviceEventAsync(request, cancellationToken: cancellationToken);

            _logger.LogInformation("Simulated device event for {DeviceId} with status {Status}", deviceId, status);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to simulate device event for {DeviceId}", deviceId);
            throw new InvalidOperationException($"Failed to simulate device event: {ex.Status.Detail}", ex);
        }
    }

    private static DeviceResponse MapToDeviceResponse(GrpcDeviceInfo device)
    {
        // Convert metadata map to JSON string for simplicity
        var metadataJson = device.Metadata.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(device.Metadata)
            : string.Empty;

        // Check if device is online (has recent heartbeat within 5 minutes)
        var isOnline = device.LastHeartbeat != null &&
                      (DateTime.UtcNow - device.LastHeartbeat.ToDateTime()).TotalMinutes < 5;

        return new DeviceResponse(
            device.DeviceId,
            device.DeviceType.ToString(),
            device.Status.ToString(),
            device.Location,
            metadataJson,
            device.LastHeartbeat?.ToDateTime(),
            isOnline);
    }

    private static DeviceResponse MapToDeviceResponse(GetDeviceStatusResponse response)
    {
        // Convert metadata map to JSON string
        var metadataJson = response.Metadata.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(response.Metadata)
            : string.Empty;

        // Check if device is online based on last heartbeat
        var isOnline = response.LastHeartbeat != null &&
                      (DateTime.UtcNow - response.LastHeartbeat.ToDateTime()).TotalMinutes < 5;

        return new DeviceResponse(
            response.DeviceId,
            response.DeviceType.ToString(),
            response.Status.ToString(),
            response.Location,
            metadataJson,
            response.LastHeartbeat?.ToDateTime(),
            isOnline);
    }

    private static DeviceResponse MapToDeviceResponse(UpdateDeviceResponse response)
    {
        return MapToDeviceResponse(response.Device);
    }
}
