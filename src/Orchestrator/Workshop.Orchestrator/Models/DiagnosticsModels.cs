namespace Workshop.Orchestrator.Models;

public record SystemHealthResponse(
    string OverallStatus,
    DateTime CheckedAt,
    List<ServiceHealthResponse> Services);

public record ServiceHealthResponse(
    string ServiceName,
    string Status,
    long? ResponseTimeMs,
    string? Message);

public record ErrorLogResponse(
    string LogId,
    string ServiceName,
    string LogLevel,
    string Message,
    string? StackTrace,
    DateTime LoggedAt);
