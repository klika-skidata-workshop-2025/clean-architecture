using DeviceService.Application.Common.Interfaces;
using DeviceService.Domain.Enums;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Workshop.Common.Errors;
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Events;

namespace DeviceService.Application.Features.Devices.Commands.UpdateDevice;

/// <summary>
/// Command to update a device's status and information.
/// </summary>
public record UpdateDeviceCommand(
    string DeviceId,
    DeviceStatus? Status = null,
    string? Name = null,
    string? Location = null,
    string? FirmwareVersion = null) : IRequest<Result>;

/// <summary>
/// Validator for UpdateDeviceCommand.
/// </summary>
public class UpdateDeviceCommandValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Invalid device status");

        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Device name must not exceed 100 characters");

        RuleFor(x => x.Location)
            .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Location))
            .WithMessage("Location must not exceed 200 characters");

        RuleFor(x => x.FirmwareVersion)
            .MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.FirmwareVersion))
            .WithMessage("Firmware version must not exceed 20 characters");
    }
}

/// <summary>
/// Handler for UpdateDeviceCommand.
/// </summary>
public class UpdateDeviceCommandHandler : IRequestHandler<UpdateDeviceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IRabbitMQPublisher _publisher;

    public UpdateDeviceCommandHandler(IApplicationDbContext context, IRabbitMQPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Result> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        // Find device
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

        if (device == null)
        {
            return CommonErrors.Device.NotFound(request.DeviceId);
        }

        var oldStatus = device.Status;

        // Update status if provided
        if (request.Status.HasValue && request.Status.Value != device.Status)
        {
            device.UpdateStatus(request.Status.Value);
        }

        // Update info if provided
        if (!string.IsNullOrWhiteSpace(request.Name) ||
            !string.IsNullOrWhiteSpace(request.Location) ||
            !string.IsNullOrWhiteSpace(request.FirmwareVersion))
        {
            device.UpdateInfo(request.Name, request.Location, request.FirmwareVersion);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Publish event if status changed
        if (request.Status.HasValue && oldStatus != device.Status)
        {
            await _publisher.PublishAsync(new DeviceStatusChangedEvent
            {
                DeviceId = device.Id,
                DeviceName = device.Name,
                DeviceType = device.Type.ToString(),
                OldStatus = oldStatus.ToString(),
                NewStatus = device.Status.ToString(),
                Location = device.Location
            }, cancellationToken);
        }

        return Result.Ok();
    }
}
