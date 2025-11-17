namespace Workshop.Orchestrator.Models;

public record RegisterDeviceRequest(
    string DeviceId,
    string DeviceType,
    string Location,
    string Metadata);

public record UpdateDeviceRequest(
    string Status,
    string? Metadata);

public record DeviceResponse(
    string DeviceId,
    string DeviceType,
    string Status,
    string Location,
    string? Metadata,
    DateTime? LastHeartbeat,
    bool IsOnline);
