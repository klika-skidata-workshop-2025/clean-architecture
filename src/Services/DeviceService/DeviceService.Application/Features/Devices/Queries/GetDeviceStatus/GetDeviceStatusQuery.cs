using DeviceService.Application.Common.Interfaces;
using DeviceService.Domain.Enums;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Workshop.Common.Errors;

namespace DeviceService.Application.Features.Devices.Queries.GetDeviceStatus;

/// <summary>
/// Query to get the status of a specific device.
/// </summary>
public record GetDeviceStatusQuery(string DeviceId) : IRequest<Result<DeviceStatusDto>>;

/// <summary>
/// Device status DTO.
/// </summary>
public record DeviceStatusDto(
    string Id,
    string Name,
    DeviceType Type,
    DeviceStatus Status,
    string Location,
    DateTime? LastHeartbeat,
    bool IsOnline,
    string FirmwareVersion,
    string IpAddress,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Validator for GetDeviceStatusQuery.
/// </summary>
public class GetDeviceStatusQueryValidator : AbstractValidator<GetDeviceStatusQuery>
{
    public GetDeviceStatusQueryValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required");
    }
}

/// <summary>
/// Handler for GetDeviceStatusQuery.
/// </summary>
public class GetDeviceStatusQueryHandler : IRequestHandler<GetDeviceStatusQuery, Result<DeviceStatusDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DeviceStatusDto>> Handle(GetDeviceStatusQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

        if (device == null)
        {
            return CommonErrors.Device.NotFound(request.DeviceId);
        }

        var dto = new DeviceStatusDto(
            device.Id,
            device.Name,
            device.Type,
            device.Status,
            device.Location,
            device.LastHeartbeat,
            device.IsOnline(),
            device.FirmwareVersion,
            device.IpAddress,
            device.CreatedAt,
            device.UpdatedAt);

        return Result.Ok(dto);
    }
}
