# Workshop.Common

Common utilities, Result pattern extensions, and shared types for the Skidata Workshop microservices.

## Overview

This library provides foundational utilities used across all workshop services:

- **FluentResults Integration** - Extension methods for converting Results to gRPC exceptions
- **Common Errors** - Predefined error types with metadata for consistent error handling
- **Extension Methods** - Utility methods for DateTime, String, and other common operations

## Installation

```bash
dotnet add package Workshop.Common
```

## Usage

### Result Pattern with FluentResults

The workshop uses FluentResults for railway-oriented programming instead of exception-based error handling.

#### Basic Example

```csharp
using FluentResults;
using Workshop.Common.Errors;

public class DeviceService
{
    public async Task<Result<Device>> GetDeviceAsync(string deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);

        if (device == null)
        {
            return Result.Fail(CommonErrors.Device.NotFound(deviceId));
        }

        return Result.Ok(device);
    }
}
```

#### gRPC Integration

```csharp
using Workshop.Common.Extensions;
using Grpc.Core;

public class DeviceGrpcService : DeviceService.DeviceServiceBase
{
    public override async Task<DeviceStatusResponse> GetDeviceStatus(
        GetDeviceStatusRequest request,
        ServerCallContext context)
    {
        var query = new GetDeviceStatusQuery(request.DeviceId);

        // MatchAsync handles both success and failure paths
        return await _mediator.Send(query).MatchAsync(
            // Success path: transform DTO to gRPC response
            dto => new DeviceStatusResponse
            {
                DeviceId = dto.DeviceId,
                Status = dto.Status
            },
            // Failure path: convert to RpcException
            error => error.ToRpcException()
        );
    }
}
```

### Common Errors

Predefined errors with consistent structure and metadata:

```csharp
using Workshop.Common.Errors;

// Device not found
var error = CommonErrors.Device.NotFound("gate-42");
// Error: "Device with ID 'gate-42' was not found."
// Metadata: { ErrorCode: "DEVICE_NOT_FOUND", DeviceId: "gate-42" }

// Invalid status
var error = CommonErrors.Device.InvalidStatus("BROKEN",
    validStatuses: new[] { "ACTIVE", "INACTIVE", "MAINTENANCE" });

// Validation error
var error = CommonErrors.Validation.RequiredField("DeviceId");

// Infrastructure error
var error = CommonErrors.Infrastructure.DatabaseConnectionFailed("Connection timeout");
```

### Extension Methods

#### DateTime Extensions

```csharp
using Workshop.Common.Extensions;

// Check if datetime is recent
if (device.LastHeartbeat.IsRecent(5))
{
    // Device sent heartbeat in the last 5 minutes
}

// Unix timestamp conversion
var timestamp = DateTime.UtcNow.ToUnixTimeSeconds();
var dateTime = DateTimeExtensions.FromUnixTimeSeconds(timestamp);

// Truncate to minute precision
var truncated = DateTime.UtcNow.TruncateTo(TimeSpan.FromMinutes(1));
```

#### String Extensions

```csharp
using Workshop.Common.Extensions;

// Check for value
if (deviceId.HasValue())
{
    // String is not null, empty, or whitespace
}

// Truncate long messages
var short = longMessage.Truncate(50, appendEllipsis: true);

// Mask sensitive data
var masked = apiKey.MaskSensitive(4);
// Result: "ABC1*********XYZ9"

// Case conversions
var snakeCase = "DeviceStatus".ToSnakeCase(); // "device_status"
var titleCase = "device status".ToTitleCase(); // "Device Status"

// Case-insensitive contains
if (errorMessage.ContainsIgnoreCase("not found"))
{
    // Handle not found error
}
```

## Error Handling Best Practices

### DO ✅

```csharp
// Return Result from business logic
public async Task<Result<Device>> UpdateDeviceAsync(string id, DeviceStatus status)
{
    var device = await _context.Devices.FindAsync(id);

    if (device == null)
    {
        return Result.Fail(CommonErrors.Device.NotFound(id));
    }

    device.UpdateStatus(status);
    await _context.SaveChangesAsync();

    return Result.Ok(device);
}

// Use MatchAsync in gRPC services
public override async Task<UpdateDeviceResponse> UpdateDevice(...)
{
    var command = new UpdateDeviceCommand(request.DeviceId, request.Status);

    return await _mediator.Send(command).MatchAsync(
        device => new UpdateDeviceResponse { Success = true },
        error => error.ToRpcException()
    );
}
```

### DON'T ❌

```csharp
// Don't throw exceptions for business logic failures
public async Task<Device> UpdateDeviceAsync(string id, DeviceStatus status)
{
    var device = await _context.Devices.FindAsync(id);

    if (device == null)
    {
        throw new NotFoundException($"Device {id} not found"); // ❌
    }

    return device;
}

// Don't create ad-hoc error messages
return Result.Fail("Device not found"); // ❌
return Result.Fail(CommonErrors.Device.NotFound(id)); // ✅
```

## Error to gRPC Status Code Mapping

The `ToRpcException()` method automatically maps error codes to gRPC status codes:

| Error Code Pattern | gRPC Status Code |
|-------------------|------------------|
| `*_NOT_FOUND` | `NotFound` |
| `INVALID_*` | `InvalidArgument` |
| `*_ALREADY_EXISTS` or `*_CONFLICT` | `AlreadyExists` |
| `*_UNAUTHORIZED` | `Unauthenticated` |
| `*_FORBIDDEN` | `PermissionDenied` |
| `*_UNAVAILABLE` | `Unavailable` |
| Others | `Internal` |

## Contributing

When adding new common errors:

1. Group them in the appropriate class (Device, Monitoring, Diagnostics, etc.)
2. Include a meaningful ErrorCode in metadata
3. Add relevant context to metadata (IDs, reasons, etc.)
4. Document the error with XML comments

Example:

```csharp
/// <summary>
/// Creates an error indicating that a device is offline.
/// </summary>
/// <param name="deviceId">The ID of the offline device</param>
/// <param name="lastSeen">When the device was last seen</param>
/// <returns>A FluentResults Error</returns>
public static Error DeviceOffline(string deviceId, DateTime lastSeen) =>
    new Error($"Device '{deviceId}' is offline. Last seen: {lastSeen:yyyy-MM-dd HH:mm:ss}")
        .WithMetadata("ErrorCode", "DEVICE_OFFLINE")
        .WithMetadata("DeviceId", deviceId)
        .WithMetadata("LastSeen", lastSeen.ToString("o"));
```

## License

MIT - See LICENSE file for details.
