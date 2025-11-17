using DiagnosticsService.Domain.Common;
using DiagnosticsService.Domain.Enums;

namespace DiagnosticsService.Domain.Entities;

/// <summary>
/// Represents an error log entry.
/// </summary>
public class ErrorLog : BaseEntity
{
    public string ServiceName { get; private set; } = string.Empty;
    public LogLevel Level { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }
    public string? Source { get; private set; }
    public string Metadata { get; private set; } = "{}";

    private ErrorLog() { }

    public static ErrorLog Create(
        string serviceName,
        LogLevel level,
        string message,
        string? stackTrace = null,
        string? source = null)
    {
        return new ErrorLog
        {
            ServiceName = serviceName,
            Level = level,
            Message = message,
            StackTrace = stackTrace,
            Source = source
        };
    }

    public void UpdateMetadata(string metadata)
    {
        Metadata = metadata;
        MarkAsUpdated();
    }
}
