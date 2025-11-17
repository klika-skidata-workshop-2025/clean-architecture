# Workshop.Proto

Protocol Buffer definitions for the Skidata Workshop microservices.

## Overview

This library contains the **contract-first** API definitions for all workshop services:

- **Device Service** - Device management (gates, lifts, counters)
- **Monitoring Service** - Real-time monitoring and alerting
- **Diagnostics Service** - Health checks and logging

These `.proto` files are compiled into C# code and distributed as a NuGet package, ensuring all services use the same contracts.

## Proto Files

### `common/common.proto`

Shared types used across all services:

- `HealthStatus` - Health status enumeration
- `Severity` - Alert severity levels
- `ErrorDetails` - Standard error structure
- `PaginationRequest`/`PaginationResponse` - Pagination support
- `TimeRange` - Time range filtering
- `SuccessResponse` - Standard success response

### `device/device.proto`

Device Service contracts:

**Service Definition:**
```protobuf
service DeviceService {
  rpc GetDeviceStatus(GetDeviceStatusRequest) returns (GetDeviceStatusResponse);
  rpc ListDevices(ListDevicesRequest) returns (ListDevicesResponse);
  rpc UpdateDevice(UpdateDeviceRequest) returns (UpdateDeviceResponse);
  rpc RegisterDevice(RegisterDeviceRequest) returns (RegisterDeviceResponse);
  rpc SimulateDeviceEvent(SimulateDeviceEventRequest) returns (SimulateDeviceEventResponse);
  rpc GetDeviceHeartbeats(GetDeviceHeartbeatsRequest) returns (GetDeviceHeartbeatsResponse);
}
```

**Key Types:**
- `DeviceStatus` - Enumeration of device states (ACTIVE, INACTIVE, MAINTENANCE, BLOCKED, OFFLINE, ERROR)
- `DeviceType` - Types of physical devices (GATE, BARRIER, TURNSTILE, LIFT, COUNTER, READER)
- `DeviceEventType` - Event types for device operations
- `DeviceInfo` - Complete device information DTO
- `DeviceHeartbeat` - Device health heartbeat record

### `monitoring/monitoring.proto`

Monitoring Service contracts:

**Service Definition:**
```protobuf
service MonitoringService {
  rpc GetActiveAlerts(GetActiveAlertsRequest) returns (GetActiveAlertsResponse);
  rpc GetAlertHistory(GetAlertHistoryRequest) returns (GetAlertHistoryResponse);
  rpc GetAlert(GetAlertRequest) returns (GetAlertResponse);
  rpc AcknowledgeAlert(AcknowledgeAlertRequest) returns (AcknowledgeAlertResponse);
  rpc ListMonitoringRules(ListMonitoringRulesRequest) returns (ListMonitoringRulesResponse);
  rpc CreateMonitoringRule(CreateMonitoringRuleRequest) returns (CreateMonitoringRuleResponse);
  rpc UpdateMonitoringRule(UpdateMonitoringRuleRequest) returns (UpdateMonitoringRuleResponse);
  rpc DeleteMonitoringRule(DeleteMonitoringRuleRequest) returns (DeleteMonitoringRuleResponse);
}
```

**Key Types:**
- `AlertStatus` - Alert lifecycle states (ACTIVE, ACKNOWLEDGED, RESOLVED, AUTO_RESOLVED)
- `RuleConditionType` - Types of monitoring conditions
- `RuleActionType` - Actions to execute when rules trigger
- `AlertInfo` - Complete alert information
- `MonitoringRuleInfo` - Monitoring rule configuration

### `diagnostics/diagnostics.proto`

Diagnostics Service contracts:

**Service Definition:**
```protobuf
service DiagnosticsService {
  rpc GetSystemHealth(GetSystemHealthRequest) returns (GetSystemHealthResponse);
  rpc GetServiceHealth(GetServiceHealthRequest) returns (GetServiceHealthResponse);
  rpc GetErrorLogs(GetErrorLogsRequest) returns (GetErrorLogsResponse);
  rpc LogError(LogErrorRequest) returns (LogErrorResponse);
  rpc GetSystemMetrics(GetSystemMetricsRequest) returns (GetSystemMetricsResponse);
  rpc GetServiceDependencies(GetServiceDependenciesRequest) returns (GetServiceDependenciesResponse);
}
```

**Key Types:**
- `LogLevel` - Log levels (TRACE, DEBUG, INFO, WARNING, ERROR, CRITICAL)
- `ComponentType` - System component types (SERVICE, DATABASE, MESSAGE_BUS, EXTERNAL_API)
- `ServiceHealth` - Service health information
- `ComponentHealth` - Infrastructure component health
- `ErrorLog` - Error log entry with exception details

## Code Generation

The `.proto` files are automatically compiled to C# code during build:

```bash
dotnet build Workshop.Proto.csproj
```

Generated files location: `obj/Debug/net8.0/` (not checked into source control)

## Usage in Services

### 1. Reference the NuGet Package

```xml
<PackageReference Include="Workshop.Proto" Version="1.0.0" />
```

### 2. Implement gRPC Service

```csharp
using Workshop.Contracts.Device;
using Grpc.Core;

public class DeviceGrpcService : DeviceService.DeviceServiceBase
{
    public override async Task<GetDeviceStatusResponse> GetDeviceStatus(
        GetDeviceStatusRequest request,
        ServerCallContext context)
    {
        // Implementation
        return new GetDeviceStatusResponse
        {
            DeviceId = request.DeviceId,
            Status = DeviceStatus.Active,
            // ... other fields
        };
    }
}
```

### 3. Use gRPC Client

```csharp
using Workshop.Contracts.Device;
using Grpc.Net.Client;

// Create channel
var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new DeviceService.DeviceServiceClient(channel);

// Call service
var response = await client.GetDeviceStatusAsync(new GetDeviceStatusRequest
{
    DeviceId = "gate-42"
});
```

## Proto Best Practices

### Field Numbering

- Never reuse field numbers
- Reserve deleted field numbers to prevent conflicts
- Use ranges 1-15 for frequently used fields (more efficient encoding)

Example:
```protobuf
message Example {
  string field1 = 1;
  // string deleted_field = 2;  // Don't reuse 2!
  reserved 2;  // Mark as reserved
  string field3 = 3;
}
```

### Backwards Compatibility

- Always use `optional` for new fields in existing messages
- Never change field types
- Never change field numbers
- Use reserved fields for deleted fields

### Naming Conventions

- **Services**: PascalCase (e.g., `DeviceService`)
- **RPCs**: PascalCase (e.g., `GetDeviceStatus`)
- **Messages**: PascalCase (e.g., `DeviceInfo`)
- **Fields**: snake_case (e.g., `device_id`)
- **Enums**: UPPER_SNAKE_CASE (e.g., `DEVICE_STATUS_ACTIVE`)

### Documentation

Always document your proto files:

```protobuf
// Service for managing devices
service DeviceService {
  // Queries the current status of a device by ID
  // Returns NotFound if the device doesn't exist
  rpc GetDeviceStatus(GetDeviceStatusRequest) returns (GetDeviceStatusResponse);
}
```

## Updating Proto Files

When updating proto definitions:

1. **Make changes** to the `.proto` file
2. **Rebuild** the Workshop.Proto project
3. **Update version** in `Workshop.Proto.csproj`
4. **Publish** new version to GitHub Packages
5. **Update references** in consuming services

```bash
# Build and pack
dotnet pack Workshop.Proto.csproj -c Release -o ./artifacts

# Publish to GitHub Packages
dotnet nuget push ./artifacts/Workshop.Proto.1.0.1.nupkg \
  --source https://nuget.pkg.github.com/klika-skidata-workshop-2025/index.json \
  --api-key $GITHUB_TOKEN
```

## gRPC Tools

### grpcurl - Test gRPC Services

With gRPC reflection enabled, you can test services using `grpcurl`:

```bash
# List services
grpcurl -plaintext localhost:5001 list

# List methods
grpcurl -plaintext localhost:5001 list workshop.device.DeviceService

# Call method
grpcurl -plaintext -d '{"device_id": "gate-42"}' \
  localhost:5001 workshop.device.DeviceService/GetDeviceStatus
```

### Postman

Postman supports gRPC natively. Import proto files and test services interactively.

### BloomRPC

GUI client for gRPC services (similar to Postman for REST).

## Versioning

Proto files follow semantic versioning:

- **Major**: Breaking changes (field type changes, deletions)
- **Minor**: Backwards-compatible additions (new fields, new RPCs)
- **Patch**: Documentation, comments, non-functional changes

## Common Patterns

### Pagination

```protobuf
import "common/common.proto";

message ListDevicesRequest {
  common.PaginationRequest pagination = 1;
}

message ListDevicesResponse {
  repeated DeviceInfo devices = 1;
  common.PaginationResponse pagination = 2;
}
```

### Time Ranges

```protobuf
import "common/common.proto";

message GetErrorLogsRequest {
  common.TimeRange time_range = 1;
}
```

### Optional Fields

Use `optional` for fields that might not be present:

```protobuf
message UpdateDeviceRequest {
  string device_id = 1;
  optional DeviceStatus status = 2;  // Only update if provided
  optional string location = 3;
}
```

## Troubleshooting

### Code Not Generated

Ensure `Grpc.Tools` package is installed and proto files are marked as `<Protobuf>` in the `.csproj`:

```xml
<ItemGroup>
  <Protobuf Include="Protos/**/*.proto" GrpcServices="Both" />
</ItemGroup>
```

### Import Not Found

Ensure imported proto files are in the correct location and the import path matches the file structure.

### Package Version Conflicts

If multiple projects reference different versions of `Workshop.Proto`, consolidate to a single version using Central Package Management in `Directory.Packages.props`.

## License

MIT - See LICENSE file for details.
