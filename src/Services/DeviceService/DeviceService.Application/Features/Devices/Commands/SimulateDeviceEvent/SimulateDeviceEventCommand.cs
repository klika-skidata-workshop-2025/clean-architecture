using DeviceService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Workshop.Common.Errors;
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Events;

namespace DeviceService.Application.Features.Devices.Commands.SimulateDeviceEvent;

/// <summary>
/// Command to simulate a device event (for testing/demo purposes).
/// </summary>
public record SimulateDeviceEventCommand(
    string DeviceId,
    string EventType,
    string EventData) : IRequest<Result>;

/// <summary>
/// Validator for SimulateDeviceEventCommand.
/// </summary>
public class SimulateDeviceEventCommandValidator : AbstractValidator<SimulateDeviceEventCommand>
{
    public SimulateDeviceEventCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required");

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Event type is required")
            .MaximumLength(50).WithMessage("Event type must not exceed 50 characters");

        RuleFor(x => x.EventData)
            .NotEmpty().WithMessage("Event data is required");
    }
}

/// <summary>
/// Handler for SimulateDeviceEventCommand.
/// </summary>
public class SimulateDeviceEventCommandHandler : IRequestHandler<SimulateDeviceEventCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IRabbitMQPublisher _publisher;

    public SimulateDeviceEventCommandHandler(IApplicationDbContext context, IRabbitMQPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Result> Handle(SimulateDeviceEventCommand request, CancellationToken cancellationToken)
    {
        // Verify device exists
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

        if (device == null)
        {
            return CommonErrors.Device.NotFound(request.DeviceId);
        }

        // Record heartbeat
        device.RecordHeartbeat();
        await _context.SaveChangesAsync(cancellationToken);

        // Publish heartbeat event
        await _publisher.PublishAsync(new DeviceHeartbeatEvent
        {
            DeviceId = device.Id,
            DeviceType = device.Type.ToString(),
            Status = device.Status.ToString(),
            Location = device.Location,
            HealthData = new Dictionary<string, string>
            {
                { "DeviceName", device.Name },
                { "FirmwareVersion", device.FirmwareVersion },
                { "IpAddress", device.IpAddress },
                { "EventType", request.EventType },
                { "EventData", request.EventData }
            }
        }, cancellationToken);

        return Result.Ok();
    }
}
