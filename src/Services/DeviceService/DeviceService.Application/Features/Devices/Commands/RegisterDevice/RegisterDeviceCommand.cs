using DeviceService.Application.Common.Interfaces;
using DeviceService.Domain.Entities;
using DeviceService.Domain.Enums;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Workshop.Common.Errors;
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Events;

namespace DeviceService.Application.Features.Devices.Commands.RegisterDevice;

/// <summary>
/// Command to register a new device.
/// </summary>
public record RegisterDeviceCommand(
    string Name,
    DeviceType Type,
    string Location,
    string IpAddress,
    string FirmwareVersion) : IRequest<Result<string>>;

/// <summary>
/// Validator for RegisterDeviceCommand.
/// </summary>
public class RegisterDeviceCommandValidator : AbstractValidator<RegisterDeviceCommand>
{
    public RegisterDeviceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Device name is required")
            .MaximumLength(100).WithMessage("Device name must not exceed 100 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid device type");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP address is required")
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$").WithMessage("Invalid IP address format");

        RuleFor(x => x.FirmwareVersion)
            .NotEmpty().WithMessage("Firmware version is required")
            .MaximumLength(20).WithMessage("Firmware version must not exceed 20 characters");
    }
}

/// <summary>
/// Handler for RegisterDeviceCommand.
/// </summary>
public class RegisterDeviceCommandHandler : IRequestHandler<RegisterDeviceCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IRabbitMQPublisher _publisher;

    public RegisterDeviceCommandHandler(IApplicationDbContext context, IRabbitMQPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Result<string>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        // Check if device with same IP already exists
        var existingDevice = await _context.Devices
            .FirstOrDefaultAsync(d => d.IpAddress == request.IpAddress, cancellationToken);

        if (existingDevice != null)
        {
            return CommonErrors.Device.AlreadyExists(request.IpAddress);
        }

        // Create new device
        var device = Device.Create(
            request.Name,
            request.Type,
            request.Location,
            request.IpAddress,
            request.FirmwareVersion);

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish event
        await _publisher.PublishAsync(new DeviceStatusChangedEvent
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            DeviceType = device.Type.ToString(),
            OldStatus = "None",
            NewStatus = device.Status.ToString(),
            Location = device.Location
        }, cancellationToken);

        return Result.Ok(device.Id);
    }
}
