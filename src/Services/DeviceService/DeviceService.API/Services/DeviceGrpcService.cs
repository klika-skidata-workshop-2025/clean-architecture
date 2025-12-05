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
using Workshop.Contracts.Device;
using Workshop.Contracts.Common;
using GrpcDeviceService = Workshop.Contracts.Device.DeviceService;

namespace DeviceService.API.Services;

/// <summary>
/// gRPC service implementation for Device Service.
/// Maps proto definitions to CQRS commands and queries.
/// </summary>
public class DeviceGrpcService : GrpcDeviceService.DeviceServiceBase
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

        return await _mediator.Send(query, context.CancellationToken).MatchAsync(
            dto =>
            {
                var response = new GetDeviceStatusResponse
                {
                    DeviceId = dto.Id,
                    DeviceName = dto.Name,
                    DeviceType = MapDeviceType(dto.Type),
                    Status = MapDeviceStatus(dto.Status),
                    Location = dto.Location ?? string.Empty,
                    LastUpdated = Timestamp.FromDateTime(dto.UpdatedAt.ToUniversalTime())
                };

                if (dto.LastHeartbeat.HasValue)
                {
                    response.LastHeartbeat = Timestamp.FromDateTime(dto.LastHeartbeat.Value.ToUniversalTime());
                }

                response.Metadata.Add("firmware_version", dto.FirmwareVersion ?? string.Empty);
                response.Metadata.Add("ip_address", dto.IpAddress ?? string.Empty);
                response.Metadata.Add("is_online", dto.IsOnline.ToString().ToLower());
                response.Metadata.Add("created_at", dto.CreatedAt.ToString("O"));

                return response;
            },
            error => error.ToRpcException()
        );
    }

    public override async Task<Workshop.Contracts.Device.ListDevicesResponse> ListDevices(
        ListDevicesRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("ListDevices called with filters - Status: {Status}, Type: {Type}",
            request.Status, request.DeviceType);

        var pageNumber = request.Pagination?.Page ?? 0;
        var pageSize = request.Pagination?.PageSize ?? 20;

        var query = new ListDevicesQuery(
            Status: request.Status != Workshop.Contracts.Device.DeviceStatus.Unspecified
                ? MapDeviceStatusToDomain(request.Status)
                : null,
            Type: request.DeviceType != Workshop.Contracts.Device.DeviceType.Unspecified
                ? MapDeviceTypeToDomain(request.DeviceType)
                : null,
            Location: !string.IsNullOrWhiteSpace(request.Location) ? request.Location : null,
            PageNumber: pageNumber > 0 ? pageNumber : 1,
            PageSize: pageSize > 0 ? pageSize : 20);

        return await _mediator.Send(query, context.CancellationToken).MatchAsync(
            dto =>
            {
                var response = new Workshop.Contracts.Device.ListDevicesResponse
                {
                    Pagination = new PaginationResponse
                    {
                        CurrentPage = dto.PageNumber,
                        PageSize = dto.PageSize,
                        TotalItems = dto.TotalCount,
                        TotalPages = dto.TotalPages,
                        HasNext = dto.PageNumber < dto.TotalPages,
                        HasPrevious = dto.PageNumber > 1
                    }
                };

                response.Devices.AddRange(dto.Devices.Select(d =>
                {
                    var deviceInfo = new DeviceInfo
                    {
                        DeviceId = d.Id,
                        DeviceName = d.Name,
                        DeviceType = MapDeviceType(d.Type),
                        Status = MapDeviceStatus(d.Status),
                        Location = d.Location ?? string.Empty,
                        CreatedAt = Timestamp.FromDateTime(d.CreatedAt.ToUniversalTime()),
                        LastUpdated = Timestamp.FromDateTime(d.UpdatedAt.ToUniversalTime())
                    };

                    if (d.LastHeartbeat.HasValue)
                    {
                        deviceInfo.LastHeartbeat = Timestamp.FromDateTime(d.LastHeartbeat.Value.ToUniversalTime());
                    }

                    deviceInfo.Metadata.Add("firmware_version", d.FirmwareVersion ?? string.Empty);
                    deviceInfo.Metadata.Add("ip_address", d.IpAddress ?? string.Empty);
                    deviceInfo.Metadata.Add("is_online", d.IsOnline.ToString().ToLower());

                    return deviceInfo;
                }));

                return response;
            },
            error => error.ToRpcException()
        );
    }

    public override async Task<UpdateDeviceResponse> UpdateDevice(
        UpdateDeviceRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("UpdateDevice called for device: {DeviceId}", request.DeviceId);

        var name = request.Metadata.TryGetValue("name", out var nameValue) ? nameValue : null;
        var firmwareVersion = request.Metadata.TryGetValue("firmware_version", out var fwValue) ? fwValue : null;

        var command = new UpdateDeviceCommand(
            request.DeviceId,
            request.HasStatus ? MapDeviceStatusToDomain(request.Status) : null,
            name,
            request.HasLocation ? request.Location : null,
            firmwareVersion);

        await _mediator.Send(command, context.CancellationToken).ThrowIfFailureAsync();

        return new UpdateDeviceResponse
        {
            Success = true,
            Message = "Device updated successfully"
        };
    }

    public override async Task<RegisterDeviceResponse> RegisterDevice(
        RegisterDeviceRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("RegisterDevice called: {DeviceName}", request.DeviceName);

        var ipAddress = request.Metadata.TryGetValue("ip_address", out var ipValue) ? ipValue : string.Empty;
        var firmwareVersion = request.Metadata.TryGetValue("firmware_version", out var fwValue) ? fwValue : string.Empty;

        var command = new RegisterDeviceCommand(
            request.DeviceName,
            MapDeviceTypeToDomain(request.DeviceType),
            request.Location,
            ipAddress,
            firmwareVersion);

        return await _mediator.Send(command, context.CancellationToken).MatchAsync(
            deviceId => new RegisterDeviceResponse
            {
                Success = true,
                Message = "Device registered successfully",
                Device = new DeviceInfo
                {
                    DeviceId = deviceId,
                    DeviceName = request.DeviceName,
                    DeviceType = request.DeviceType,
                    Status = Workshop.Contracts.Device.DeviceStatus.Inactive,
                    Location = request.Location
                }
            },
            error => error.ToRpcException()
        );
    }

    public override async Task<SimulateDeviceEventResponse> SimulateDeviceEvent(
        SimulateDeviceEventRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("SimulateDeviceEvent called for device: {DeviceId}, EventType: {EventType}",
            request.DeviceId, request.EventType);

        var eventData = string.Join(";", request.EventData.Select(kv => $"{kv.Key}={kv.Value}"));

        var command = new SimulateDeviceEventCommand(
            request.DeviceId,
            request.EventType.ToString(),
            eventData);

        await _mediator.Send(command, context.CancellationToken).ThrowIfFailureAsync();

        return new SimulateDeviceEventResponse
        {
            Success = true,
            Message = "Event simulated successfully",
            EventId = Guid.NewGuid().ToString()
        };
    }

    public override async Task<GetDeviceHeartbeatsResponse> GetDeviceHeartbeats(
        GetDeviceHeartbeatsRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetDeviceHeartbeats called for device: {DeviceId}", request.DeviceId);

        var startTime = request.TimeRange?.Start?.ToDateTime();
        var endTime = request.TimeRange?.End?.ToDateTime();

        var query = new GetDeviceHeartbeatsQuery(
            request.DeviceId,
            startTime,
            endTime);

        return await _mediator.Send(query, context.CancellationToken).MatchAsync(
            dto =>
            {
                var response = new GetDeviceHeartbeatsResponse();

                response.Heartbeats.AddRange(dto.RecentHeartbeats.Select(h => new DeviceHeartbeat
                {
                    DeviceId = dto.DeviceId,
                    Timestamp = Timestamp.FromDateTime(h.Timestamp.ToUniversalTime()),
                    Status = MapDeviceStatus(System.Enum.TryParse<Domain.Enums.DeviceStatus>(h.Status, out var status)
                        ? status
                        : Domain.Enums.DeviceStatus.Active)
                }));

                return response;
            },
            error => error.ToRpcException()
        );
    }

    private static Workshop.Contracts.Device.DeviceStatus MapDeviceStatus(Domain.Enums.DeviceStatus status)
    {
        return status switch
        {
            Domain.Enums.DeviceStatus.Active => Workshop.Contracts.Device.DeviceStatus.Active,
            Domain.Enums.DeviceStatus.Inactive => Workshop.Contracts.Device.DeviceStatus.Inactive,
            Domain.Enums.DeviceStatus.Maintenance => Workshop.Contracts.Device.DeviceStatus.Maintenance,
            Domain.Enums.DeviceStatus.Blocked => Workshop.Contracts.Device.DeviceStatus.Blocked,
            Domain.Enums.DeviceStatus.Offline => Workshop.Contracts.Device.DeviceStatus.Offline,
            Domain.Enums.DeviceStatus.Error => Workshop.Contracts.Device.DeviceStatus.Error,
            _ => Workshop.Contracts.Device.DeviceStatus.Unspecified
        };
    }

    private static Domain.Enums.DeviceStatus MapDeviceStatusToDomain(Workshop.Contracts.Device.DeviceStatus status)
    {
        return status switch
        {
            Workshop.Contracts.Device.DeviceStatus.Active => Domain.Enums.DeviceStatus.Active,
            Workshop.Contracts.Device.DeviceStatus.Inactive => Domain.Enums.DeviceStatus.Inactive,
            Workshop.Contracts.Device.DeviceStatus.Maintenance => Domain.Enums.DeviceStatus.Maintenance,
            Workshop.Contracts.Device.DeviceStatus.Blocked => Domain.Enums.DeviceStatus.Blocked,
            Workshop.Contracts.Device.DeviceStatus.Offline => Domain.Enums.DeviceStatus.Offline,
            Workshop.Contracts.Device.DeviceStatus.Error => Domain.Enums.DeviceStatus.Error,
            _ => Domain.Enums.DeviceStatus.Inactive
        };
    }

    private static Workshop.Contracts.Device.DeviceType MapDeviceType(Domain.Enums.DeviceType type)
    {
        return type switch
        {
            Domain.Enums.DeviceType.Gate => Workshop.Contracts.Device.DeviceType.Gate,
            Domain.Enums.DeviceType.Lift => Workshop.Contracts.Device.DeviceType.Lift,
            Domain.Enums.DeviceType.Counter => Workshop.Contracts.Device.DeviceType.Counter,
            Domain.Enums.DeviceType.Control => Workshop.Contracts.Device.DeviceType.Turnstile,
            _ => Workshop.Contracts.Device.DeviceType.Unspecified
        };
    }

    private static Domain.Enums.DeviceType MapDeviceTypeToDomain(Workshop.Contracts.Device.DeviceType type)
    {
        return type switch
        {
            Workshop.Contracts.Device.DeviceType.Gate => Domain.Enums.DeviceType.Gate,
            Workshop.Contracts.Device.DeviceType.Lift => Domain.Enums.DeviceType.Lift,
            Workshop.Contracts.Device.DeviceType.Counter => Domain.Enums.DeviceType.Counter,
            Workshop.Contracts.Device.DeviceType.Turnstile => Domain.Enums.DeviceType.Control,
            Workshop.Contracts.Device.DeviceType.Barrier => Domain.Enums.DeviceType.Gate,
            Workshop.Contracts.Device.DeviceType.Reader => Domain.Enums.DeviceType.Control,
            _ => Domain.Enums.DeviceType.Gate
        };
    }
}
