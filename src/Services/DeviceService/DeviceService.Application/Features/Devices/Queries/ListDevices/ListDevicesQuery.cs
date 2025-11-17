using DeviceService.Application.Common.Interfaces;
using DeviceService.Application.Features.Devices.Queries.GetDeviceStatus;
using DeviceService.Domain.Enums;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeviceService.Application.Features.Devices.Queries.ListDevices;

/// <summary>
/// Query to list devices with optional filtering.
/// </summary>
public record ListDevicesQuery(
    DeviceStatus? Status = null,
    DeviceType? Type = null,
    string? Location = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<ListDevicesResponse>>;

/// <summary>
/// Response containing a list of devices with pagination.
/// </summary>
public record ListDevicesResponse(
    List<DeviceStatusDto> Devices,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Validator for ListDevicesQuery.
/// </summary>
public class ListDevicesQueryValidator : AbstractValidator<ListDevicesQuery>
{
    public ListDevicesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Invalid device status");

        RuleFor(x => x.Type)
            .IsInEnum().When(x => x.Type.HasValue)
            .WithMessage("Invalid device type");
    }
}

/// <summary>
/// Handler for ListDevicesQuery.
/// </summary>
public class ListDevicesQueryHandler : IRequestHandler<ListDevicesQuery, Result<ListDevicesResponse>>
{
    private readonly IApplicationDbContext _context;

    public ListDevicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ListDevicesResponse>> Handle(ListDevicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Devices.AsQueryable();

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(d => d.Type == request.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(d => d.Location.Contains(request.Location));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Apply pagination
        var devices = await query
            .OrderBy(d => d.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DeviceStatusDto(
                d.Id,
                d.Name,
                d.Type,
                d.Status,
                d.Location,
                d.LastHeartbeat,
                d.IsOnline(),
                d.FirmwareVersion,
                d.IpAddress,
                d.CreatedAt,
                d.UpdatedAt))
            .ToListAsync(cancellationToken);

        var response = new ListDevicesResponse(
            devices,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Ok(response);
    }
}
