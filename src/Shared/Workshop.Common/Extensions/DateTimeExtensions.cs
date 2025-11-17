namespace Workshop.Common.Extensions;

/// <summary>
/// Extension methods for DateTime operations.
/// Provides utility methods for common date/time manipulations used across the workshop services.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a DateTime to Unix timestamp (seconds since epoch).
    /// Useful for interop with systems that use Unix timestamps.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Unix timestamp in seconds</returns>
    /// <example>
    /// <code>
    /// var timestamp = DateTime.UtcNow.ToUnixTimeSeconds();
    /// </code>
    /// </example>
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts a DateTime to Unix timestamp in milliseconds.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Unix timestamp in milliseconds</returns>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Converts a Unix timestamp (seconds) to DateTime (UTC).
    /// </summary>
    /// <param name="unixTimeSeconds">Unix timestamp in seconds</param>
    /// <returns>DateTime in UTC</returns>
    /// <example>
    /// <code>
    /// var dateTime = DateTimeExtensions.FromUnixTimeSeconds(1234567890);
    /// </code>
    /// </example>
    public static DateTime FromUnixTimeSeconds(long unixTimeSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
    }

    /// <summary>
    /// Checks if a DateTime is within a specified time range.
    /// Useful for validation and time-based queries.
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <param name="start">Start of the range (inclusive)</param>
    /// <param name="end">End of the range (inclusive)</param>
    /// <returns>True if the DateTime falls within the range</returns>
    /// <example>
    /// <code>
    /// if (device.LastSeen.IsWithinRange(yesterday, now))
    /// {
    ///     // Device is active
    /// }
    /// </code>
    /// </example>
    public static bool IsWithinRange(this DateTime dateTime, DateTime start, DateTime end)
    {
        return dateTime >= start && dateTime <= end;
    }

    /// <summary>
    /// Truncates a DateTime to the specified precision.
    /// Useful for comparing dates without considering time components.
    /// </summary>
    /// <param name="dateTime">The DateTime to truncate</param>
    /// <param name="precision">The precision to truncate to (e.g., TimeSpan.FromSeconds(1))</param>
    /// <returns>Truncated DateTime</returns>
    /// <example>
    /// <code>
    /// var truncated = DateTime.UtcNow.TruncateTo(TimeSpan.FromMinutes(1));
    /// // Result: 2025-11-17 10:30:00 (seconds truncated)
    /// </code>
    /// </example>
    public static DateTime TruncateTo(this DateTime dateTime, TimeSpan precision)
    {
        return new DateTime(dateTime.Ticks - (dateTime.Ticks % precision.Ticks), dateTime.Kind);
    }

    /// <summary>
    /// Checks if a DateTime is considered "recent" (within the last N minutes).
    /// Useful for health checks and activity tracking.
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <param name="withinMinutes">Number of minutes to consider as "recent"</param>
    /// <returns>True if the DateTime is within the specified minutes from now</returns>
    /// <example>
    /// <code>
    /// if (device.LastHeartbeat.IsRecent(5))
    /// {
    ///     // Device sent heartbeat in the last 5 minutes
    /// }
    /// </code>
    /// </example>
    public static bool IsRecent(this DateTime dateTime, int withinMinutes = 5)
    {
        return DateTime.UtcNow.Subtract(dateTime).TotalMinutes <= withinMinutes;
    }
}
