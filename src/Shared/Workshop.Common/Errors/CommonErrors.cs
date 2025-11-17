using FluentResults;

namespace Workshop.Common.Errors;

/// <summary>
/// Predefined common errors used across all workshop services.
/// This class provides factory methods for creating consistent, well-structured errors
/// with appropriate metadata for debugging and client error handling.
/// </summary>
/// <remarks>
/// Each error includes:
/// - ErrorCode: A machine-readable code for programmatic handling
/// - Message: A human-readable description
/// - Metadata: Additional context for logging and debugging
/// </remarks>
public static class CommonErrors
{
    /// <summary>
    /// Errors related to Device Service operations.
    /// </summary>
    public static class Device
    {
        /// <summary>
        /// Creates an error indicating that a device was not found.
        /// Maps to gRPC StatusCode.NotFound when converted to RpcException.
        /// </summary>
        /// <param name="deviceId">The ID of the device that was not found</param>
        /// <returns>A FluentResults Error with NOT_FOUND metadata</returns>
        /// <example>
        /// <code>
        /// var device = await _context.Devices.FindAsync(deviceId);
        /// if (device == null)
        /// {
        ///     return Result.Fail(CommonErrors.Device.NotFound(deviceId));
        /// }
        /// </code>
        /// </example>
        public static Error NotFound(string deviceId) =>
            new Error($"Device with ID '{deviceId}' was not found.")
                .WithMetadata("ErrorCode", "DEVICE_NOT_FOUND")
                .WithMetadata("DeviceId", deviceId);

        /// <summary>
        /// Creates an error indicating that a device status is invalid.
        /// Maps to gRPC StatusCode.InvalidArgument when converted to RpcException.
        /// </summary>
        /// <param name="status">The invalid status value</param>
        /// <param name="validStatuses">Optional list of valid statuses for debugging</param>
        /// <returns>A FluentResults Error with INVALID metadata</returns>
        public static Error InvalidStatus(string status, string[]? validStatuses = null)
        {
            var error = new Error($"Invalid device status: '{status}'")
                .WithMetadata("ErrorCode", "INVALID_DEVICE_STATUS")
                .WithMetadata("ProvidedStatus", status);

            if (validStatuses != null && validStatuses.Length > 0)
            {
                error.WithMetadata("ValidStatuses", string.Join(", ", validStatuses));
            }

            return error;
        }

        /// <summary>
        /// Creates an error indicating that a device already exists.
        /// Maps to gRPC StatusCode.AlreadyExists when converted to RpcException.
        /// </summary>
        /// <param name="deviceId">The ID of the existing device</param>
        /// <returns>A FluentResults Error with ALREADY_EXISTS metadata</returns>
        public static Error AlreadyExists(string deviceId) =>
            new Error($"Device with ID '{deviceId}' already exists.")
                .WithMetadata("ErrorCode", "DEVICE_ALREADY_EXISTS")
                .WithMetadata("DeviceId", deviceId);

        /// <summary>
        /// Creates an error indicating that a device is in an invalid state for the requested operation.
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <param name="currentStatus">The current status of the device</param>
        /// <param name="operation">The operation that was attempted</param>
        /// <returns>A FluentResults Error with CONFLICT metadata</returns>
        public static Error InvalidStateForOperation(string deviceId, string currentStatus, string operation) =>
            new Error($"Device '{deviceId}' is in status '{currentStatus}' and cannot perform operation '{operation}'.")
                .WithMetadata("ErrorCode", "DEVICE_INVALID_STATE")
                .WithMetadata("DeviceId", deviceId)
                .WithMetadata("CurrentStatus", currentStatus)
                .WithMetadata("Operation", operation);
    }

    /// <summary>
    /// Errors related to Monitoring Service operations.
    /// </summary>
    public static class Monitoring
    {
        /// <summary>
        /// Creates an error indicating that an alert was not found.
        /// </summary>
        /// <param name="alertId">The ID of the alert that was not found</param>
        /// <returns>A FluentResults Error with NOT_FOUND metadata</returns>
        public static Error AlertNotFound(string alertId) =>
            new Error($"Alert with ID '{alertId}' was not found.")
                .WithMetadata("ErrorCode", "ALERT_NOT_FOUND")
                .WithMetadata("AlertId", alertId);

        /// <summary>
        /// Creates an error indicating that an alert is already acknowledged.
        /// </summary>
        /// <param name="alertId">The ID of the alert</param>
        /// <returns>A FluentResults Error with CONFLICT metadata</returns>
        public static Error AlertAlreadyAcknowledged(string alertId) =>
            new Error($"Alert with ID '{alertId}' has already been acknowledged.")
                .WithMetadata("ErrorCode", "ALERT_ALREADY_ACKNOWLEDGED")
                .WithMetadata("AlertId", alertId);

        /// <summary>
        /// Creates an error indicating that a monitoring rule was not found.
        /// </summary>
        /// <param name="ruleId">The ID of the rule that was not found</param>
        /// <returns>A FluentResults Error with NOT_FOUND metadata</returns>
        public static Error RuleNotFound(string ruleId) =>
            new Error($"Monitoring rule with ID '{ruleId}' was not found.")
                .WithMetadata("ErrorCode", "RULE_NOT_FOUND")
                .WithMetadata("RuleId", ruleId);

        /// <summary>
        /// Creates an error indicating invalid rule configuration.
        /// </summary>
        /// <param name="reason">The reason the rule is invalid</param>
        /// <returns>A FluentResults Error with INVALID metadata</returns>
        public static Error InvalidRule(string reason) =>
            new Error($"Invalid monitoring rule: {reason}")
                .WithMetadata("ErrorCode", "INVALID_MONITORING_RULE")
                .WithMetadata("Reason", reason);
    }

    /// <summary>
    /// Errors related to Diagnostics Service operations.
    /// </summary>
    public static class Diagnostics
    {
        /// <summary>
        /// Creates an error indicating that health check failed.
        /// </summary>
        /// <param name="serviceName">The name of the unhealthy service</param>
        /// <param name="reason">The reason for the health check failure</param>
        /// <returns>A FluentResults Error with health check metadata</returns>
        public static Error HealthCheckFailed(string serviceName, string reason) =>
            new Error($"Health check failed for service '{serviceName}': {reason}")
                .WithMetadata("ErrorCode", "HEALTH_CHECK_FAILED")
                .WithMetadata("ServiceName", serviceName)
                .WithMetadata("Reason", reason);

        /// <summary>
        /// Creates an error indicating that error log retrieval failed.
        /// </summary>
        /// <param name="reason">The reason for the failure</param>
        /// <returns>A FluentResults Error</returns>
        public static Error ErrorLogRetrievalFailed(string reason) =>
            new Error($"Failed to retrieve error logs: {reason}")
                .WithMetadata("ErrorCode", "ERROR_LOG_RETRIEVAL_FAILED")
                .WithMetadata("Reason", reason);
    }

    /// <summary>
    /// Generic validation errors.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Creates an error indicating that a required field is missing.
        /// </summary>
        /// <param name="fieldName">The name of the missing field</param>
        /// <returns>A FluentResults Error with INVALID metadata</returns>
        public static Error RequiredField(string fieldName) =>
            new Error($"The field '{fieldName}' is required.")
                .WithMetadata("ErrorCode", "VALIDATION_REQUIRED_FIELD")
                .WithMetadata("FieldName", fieldName);

        /// <summary>
        /// Creates an error indicating that a field value is out of range.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>A FluentResults Error with INVALID metadata</returns>
        public static Error OutOfRange(string fieldName, object min, object max) =>
            new Error($"The field '{fieldName}' must be between {min} and {max}.")
                .WithMetadata("ErrorCode", "VALIDATION_OUT_OF_RANGE")
                .WithMetadata("FieldName", fieldName)
                .WithMetadata("Min", min.ToString() ?? "")
                .WithMetadata("Max", max.ToString() ?? "");

        /// <summary>
        /// Creates an error indicating that a field value has invalid format.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="expectedFormat">The expected format description</param>
        /// <returns>A FluentResults Error with INVALID metadata</returns>
        public static Error InvalidFormat(string fieldName, string expectedFormat) =>
            new Error($"The field '{fieldName}' has an invalid format. Expected: {expectedFormat}")
                .WithMetadata("ErrorCode", "VALIDATION_INVALID_FORMAT")
                .WithMetadata("FieldName", fieldName)
                .WithMetadata("ExpectedFormat", expectedFormat);
    }

    /// <summary>
    /// Infrastructure-related errors.
    /// </summary>
    public static class Infrastructure
    {
        /// <summary>
        /// Creates an error indicating database connection failure.
        /// </summary>
        /// <param name="reason">The reason for the connection failure</param>
        /// <returns>A FluentResults Error with UNAVAILABLE metadata</returns>
        public static Error DatabaseConnectionFailed(string reason) =>
            new Error($"Database connection failed: {reason}")
                .WithMetadata("ErrorCode", "DATABASE_UNAVAILABLE")
                .WithMetadata("Reason", reason);

        /// <summary>
        /// Creates an error indicating message bus connection failure.
        /// </summary>
        /// <param name="reason">The reason for the connection failure</param>
        /// <returns>A FluentResults Error with UNAVAILABLE metadata</returns>
        public static Error MessageBusConnectionFailed(string reason) =>
            new Error($"Message bus connection failed: {reason}")
                .WithMetadata("ErrorCode", "MESSAGE_BUS_UNAVAILABLE")
                .WithMetadata("Reason", reason);

        /// <summary>
        /// Creates an error indicating external service is unavailable.
        /// </summary>
        /// <param name="serviceName">The name of the unavailable service</param>
        /// <param name="reason">The reason for unavailability</param>
        /// <returns>A FluentResults Error with UNAVAILABLE metadata</returns>
        public static Error ExternalServiceUnavailable(string serviceName, string reason) =>
            new Error($"External service '{serviceName}' is unavailable: {reason}")
                .WithMetadata("ErrorCode", "EXTERNAL_SERVICE_UNAVAILABLE")
                .WithMetadata("ServiceName", serviceName)
                .WithMetadata("Reason", reason);
    }
}
