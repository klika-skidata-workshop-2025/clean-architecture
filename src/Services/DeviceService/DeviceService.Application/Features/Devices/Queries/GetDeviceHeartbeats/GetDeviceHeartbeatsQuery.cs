using DeviceService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Workshop.Common.Errors;

namespace DeviceService.Application.Features.Devices.Queries.GetDeviceHeartbeats;

/// <summary>
/// Query to get heartbeat history for a device.
/// </summary>
public record GetDeviceHeartbeatsQuery(
    string DeviceId,
    DateTime? StartTime = null,
    DateTime? EndTime = null) : IRequest<Result<DeviceHeartbeatsResponse>>;

/// <summary>
/// Response containing device heartbeat information.
/// </summary>
public record DeviceHeartbeatsResponse(
    string DeviceId,
    string DeviceName,
    DateTime? LastHeartbeat,
    bool IsOnline,
    TimeSpan? TimeSinceLastHeartbeat,
    List<HeartbeatRecord> RecentHeartbeats);

/// <summary>
/// Individual heartbeat record.
/// </summary>
public record HeartbeatRecord(
    DateTime Timestamp,
    string Status);

/// <summary>
/// Validator for GetDeviceHeartbeatsQuery.
/// </summary>
public class GetDeviceHeartbeatsQueryValidator : AbstractValidator<GetDeviceHeartbeatsQuery>
{
    public GetDeviceHeartbeatsQueryValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("End time must be after start time");
    }
}

/// <summary>
/// Handler for GetDeviceHeartbeatsQuery.
/// </summary>
public class GetDeviceHeartbeatsQueryHandler : IRequestHandler<GetDeviceHeartbeatsQuery, Result<DeviceHeartbeatsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceHeartbeatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DeviceHeartbeatsResponse>> Handle(GetDeviceHeartbeatsQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

        if (device == null)
        {
            return CommonErrors.Device.NotFound(request.DeviceId);
        }

        // Calculate time since last heartbeat
        TimeSpan? timeSinceLastHeartbeat = null;
        if (device.LastHeartbeat.HasValue)
        {
            timeSinceLastHeartbeat = DateTime.UtcNow - device.LastHeartbeat.Value;
        }

        // For now, we only have the latest heartbeat stored in the Device entity
        // In a production system, you might want to store heartbeat history in a separate table
        var recentHeartbeats = new List<HeartbeatRecord>();
        if (device.LastHeartbeat.HasValue)
        {
            var includeHeartbeat = true;

            // Apply time filters if provided
            if (request.StartTime.HasValue && device.LastHeartbeat.Value < request.StartTime.Value)
            {
                includeHeartbeat = false;
            }

            if (request.EndTime.HasValue && device.LastHeartbeat.Value > request.EndTime.Value)
            {
                includeHeartbeat = false;
            }

            if (includeHeartbeat)
            {
                recentHeartbeats.Add(new HeartbeatRecord(
                    device.LastHeartbeat.Value,
                    device.Status.ToString()));
            }
        }

        var response = new DeviceHeartbeatsResponse(
            device.Id,
            device.Name,
            device.LastHeartbeat,
            device.IsOnline(),
            timeSinceLastHeartbeat,
            recentHeartbeats);

        return Result.Ok(response);
    }
}
