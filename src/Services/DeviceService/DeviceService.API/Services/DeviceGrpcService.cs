using DeviceService.Application.Features.Devices.Commands.RegisterDevice;
using DeviceService.Application.Features.Devices.Commands.SimulateDeviceEvent;
using DeviceService.Application.Features.Devices.Commands.UpdateDevice;
using DeviceService.Application.Features.Devices.Queries.GetDeviceHeartbeats;
using DeviceService.Application.Features.Devices.Queries.GetDeviceStatus;
using DeviceService.Application.Features.Devices.Queries.ListDevices;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Workshop.Common.Extensions;
using Workshop.Proto.Device;

namespace DeviceService.API.Services;

/// <summary>
/// gRPC service implementation for Device Service.
/// Maps proto definitions to CQRS commands and queries.
/// </summary>
public class DeviceGrpcService : DeviceService.DeviceServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeviceGrpcService> _logger;

    public DeviceGrpcService(IMediator mediator, ILogger<DeviceGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<GetDeviceStatusResponse> GetDeviceStatus(
        GetDeviceStatusRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetDeviceStatus called for device: {DeviceId}", request.DeviceId);

        var query = new GetDeviceStatusQuery(request.DeviceId);
        var result = await _mediator.Send(query, context.CancellationToken);

        return await result.MatchAsync(
            dto => Task.FromResult(new GetDeviceStatusResponse
            {
                DeviceId = dto.Id,
                Name = dto.Name,
                Type = MapDeviceType(dto.Type),
                Status = MapDeviceStatus(dto.Status),
                Location = dto.Location,
                LastHeartbeat = dto.LastHeartbeat.HasValue
                    ? Timestamp.FromDateTime(dto.LastHeartbeat.Value.ToUniversalTime())
                    : null,
                IsOnline = dto.IsOnline,
                FirmwareVersion = dto.FirmwareVersion,
                IpAddress = dto.IpAddress,
                CreatedAt = Timestamp.FromDateTime(dto.CreatedAt.ToUniversalTime()),
                UpdatedAt = Timestamp.FromDateTime(dto.UpdatedAt.ToUniversalTime())
            }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<ListDevicesResponse> ListDevices(
        ListDevicesRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("ListDevices called with filters - Status: {Status}, Type: {Type}",
            request.Status, request.Type);

        var query = new ListDevicesQuery(
            Status: request.Status != Workshop.Proto.Device.DeviceStatus.Unspecified
                ? MapDeviceStatusToDomain(request.Status)
                : null,
            Type: request.Type != Workshop.Proto.Device.DeviceType.Unspecified
                ? MapDeviceTypeToDomain(request.Type)
                : null,
            Location: !string.IsNullOrWhiteSpace(request.Location) ? request.Location : null,
            PageNumber: request.PageNumber > 0 ? request.PageNumber : 1,
            PageSize: request.PageSize > 0 ? request.PageSize : 20);

        var result = await _mediator.Send(query, context.CancellationToken);

        return await result.MatchAsync(
            dto =>
            {
                var response = new ListDevicesResponse
                {
                    TotalCount = dto.TotalCount,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize,
                    TotalPages = dto.TotalPages
                };

                response.Devices.AddRange(dto.Devices.Select(d => new DeviceInfo
                {
                    DeviceId = d.Id,
                    Name = d.Name,
                    Type = MapDeviceType(d.Type),
                    Status = MapDeviceStatus(d.Status),
                    Location = d.Location,
                    LastHeartbeat = d.LastHeartbeat.HasValue
                        ? Timestamp.FromDateTime(d.LastHeartbeat.Value.ToUniversalTime())
                        : null,
                    IsOnline = d.IsOnline,
                    FirmwareVersion = d.FirmwareVersion,
                    IpAddress = d.IpAddress
                }));

                return Task.FromResult(response);
            },
            error => throw error.ToRpcException()
        );
    }

    public override async Task<UpdateDeviceResponse> UpdateDevice(
        UpdateDeviceRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("UpdateDevice called for device: {DeviceId}", request.DeviceId);

        var command = new UpdateDeviceCommand(
            request.DeviceId,
            request.Status != Workshop.Proto.Device.DeviceStatus.Unspecified
                ? MapDeviceStatusToDomain(request.Status)
                : null,
            !string.IsNullOrWhiteSpace(request.Name) ? request.Name : null,
            !string.IsNullOrWhiteSpace(request.Location) ? request.Location : null,
            !string.IsNullOrWhiteSpace(request.FirmwareVersion) ? request.FirmwareVersion : null);

        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            () => Task.FromResult(new UpdateDeviceResponse { Success = true }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<RegisterDeviceResponse> RegisterDevice(
        RegisterDeviceRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("RegisterDevice called: {DeviceName}", request.Name);

        var command = new RegisterDeviceCommand(
            request.Name,
            MapDeviceTypeToDomain(request.Type),
            request.Location,
            request.IpAddress,
            request.FirmwareVersion);

        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            deviceId => Task.FromResult(new RegisterDeviceResponse
            {
                DeviceId = deviceId,
                Success = true
            }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<SimulateDeviceEventResponse> SimulateDeviceEvent(
        SimulateDeviceEventRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("SimulateDeviceEvent called for device: {DeviceId}, EventType: {EventType}",
            request.DeviceId, request.EventType);

        var command = new SimulateDeviceEventCommand(
            request.DeviceId,
            request.EventType.ToString(),
            request.EventData);

        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            () => Task.FromResult(new SimulateDeviceEventResponse { Success = true }),
            error => throw error.ToRpcException()
        );
    }

    public override async Task<GetDeviceHeartbeatsResponse> GetDeviceHeartbeats(
        GetDeviceHeartbeatsRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetDeviceHeartbeats called for device: {DeviceId}", request.DeviceId);

        var query = new GetDeviceHeartbeatsQuery(
            request.DeviceId,
            request.StartTime?.ToDateTime(),
            request.EndTime?.ToDateTime());

        var result = await _mediator.Send(query, context.CancellationToken);

        return await result.MatchAsync(
            dto =>
            {
                var response = new GetDeviceHeartbeatsResponse
                {
                    DeviceId = dto.DeviceId,
                    DeviceName = dto.DeviceName,
                    LastHeartbeat = dto.LastHeartbeat.HasValue
                        ? Timestamp.FromDateTime(dto.LastHeartbeat.Value.ToUniversalTime())
                        : null,
                    IsOnline = dto.IsOnline,
                    TimeSinceLastHeartbeatSeconds = dto.TimeSinceLastHeartbeat?.TotalSeconds ?? 0
                };

                response.RecentHeartbeats.AddRange(dto.RecentHeartbeats.Select(h => new HeartbeatInfo
                {
                    Timestamp = Timestamp.FromDateTime(h.Timestamp.ToUniversalTime()),
                    Status = h.Status
                }));

                return Task.FromResult(response);
            },
            error => throw error.ToRpcException()
        );
    }

    // Helper methods to map between proto and domain enums

    private static Workshop.Proto.Device.DeviceStatus MapDeviceStatus(Domain.Enums.DeviceStatus status)
    {
        return status switch
        {
            Domain.Enums.DeviceStatus.Active => Workshop.Proto.Device.DeviceStatus.Active,
            Domain.Enums.DeviceStatus.Inactive => Workshop.Proto.Device.DeviceStatus.Inactive,
            Domain.Enums.DeviceStatus.Maintenance => Workshop.Proto.Device.DeviceStatus.Maintenance,
            Domain.Enums.DeviceStatus.Blocked => Workshop.Proto.Device.DeviceStatus.Blocked,
            Domain.Enums.DeviceStatus.Offline => Workshop.Proto.Device.DeviceStatus.Offline,
            Domain.Enums.DeviceStatus.Error => Workshop.Proto.Device.DeviceStatus.Error,
            _ => Workshop.Proto.Device.DeviceStatus.Unspecified
        };
    }

    private static Domain.Enums.DeviceStatus MapDeviceStatusToDomain(Workshop.Proto.Device.DeviceStatus status)
    {
        return status switch
        {
            Workshop.Proto.Device.DeviceStatus.Active => Domain.Enums.DeviceStatus.Active,
            Workshop.Proto.Device.DeviceStatus.Inactive => Domain.Enums.DeviceStatus.Inactive,
            Workshop.Proto.Device.DeviceStatus.Maintenance => Domain.Enums.DeviceStatus.Maintenance,
            Workshop.Proto.Device.DeviceStatus.Blocked => Domain.Enums.DeviceStatus.Blocked,
            Workshop.Proto.Device.DeviceStatus.Offline => Domain.Enums.DeviceStatus.Offline,
            Workshop.Proto.Device.DeviceStatus.Error => Domain.Enums.DeviceStatus.Error,
            _ => Domain.Enums.DeviceStatus.Inactive
        };
    }

    private static Workshop.Proto.Device.DeviceType MapDeviceType(Domain.Enums.DeviceType type)
    {
        return type switch
        {
            Domain.Enums.DeviceType.Gate => Workshop.Proto.Device.DeviceType.Gate,
            Domain.Enums.DeviceType.Lift => Workshop.Proto.Device.DeviceType.Lift,
            Domain.Enums.DeviceType.Counter => Workshop.Proto.Device.DeviceType.Counter,
            Domain.Enums.DeviceType.Control => Workshop.Proto.Device.DeviceType.Control,
            _ => Workshop.Proto.Device.DeviceType.Unspecified
        };
    }

    private static Domain.Enums.DeviceType MapDeviceTypeToDomain(Workshop.Proto.Device.DeviceType type)
    {
        return type switch
        {
            Workshop.Proto.Device.DeviceType.Gate => Domain.Enums.DeviceType.Gate,
            Workshop.Proto.Device.DeviceType.Lift => Domain.Enums.DeviceType.Lift,
            Workshop.Proto.Device.DeviceType.Counter => Domain.Enums.DeviceType.Counter,
            Workshop.Proto.Device.DeviceType.Control => Domain.Enums.DeviceType.Control,
            _ => Domain.Enums.DeviceType.Gate
        };
    }
}
