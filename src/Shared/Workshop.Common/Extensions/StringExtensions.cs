namespace Workshop.Common.Extensions;

/// <summary>
/// Extension methods for string operations.
/// Provides utility methods for common string manipulations and validations.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if a string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if null, empty, or whitespace</returns>
    /// <example>
    /// <code>
    /// if (deviceId.IsNullOrWhiteSpace())
    /// {
    ///     return Result.Fail("Device ID is required");
    /// }
    /// </code>
    /// </example>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Checks if a string has a value (not null, empty, or whitespace).
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if the string has a value</returns>
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Truncates a string to the specified maximum length.
    /// Useful for limiting log messages or display text.
    /// </summary>
    /// <param name="value">The string to truncate</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <param name="appendEllipsis">Whether to append "..." to truncated strings</param>
    /// <returns>Truncated string</returns>
    /// <example>
    /// <code>
    /// var short = longMessage.Truncate(50, appendEllipsis: true);
    /// // Result: "This is a very long message that needs to be..."
    /// </code>
    /// </example>
    public static string Truncate(this string value, int maxLength, bool appendEllipsis = false)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        var truncated = value[..maxLength];
        return appendEllipsis ? $"{truncated}..." : truncated;
    }

    /// <summary>
    /// Converts a string to Title Case (first letter of each word capitalized).
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>String in Title Case</returns>
    /// <example>
    /// <code>
    /// var title = "device status changed".ToTitleCase();
    /// // Result: "Device Status Changed"
    /// </code>
    /// </example>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLower());
    }

    /// <summary>
    /// Converts a string to snake_case.
    /// Useful for converting C# property names to database column names.
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>String in snake_case</returns>
    /// <example>
    /// <code>
    /// var snakeCase = "DeviceStatus".ToSnakeCase();
    /// // Result: "device_status"
    /// </code>
    /// </example>
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return string.Concat(
            value.Select((c, i) =>
                i > 0 && char.IsUpper(c)
                    ? $"_{c}"
                    : c.ToString()
            )
        ).ToLower();
    }

    /// <summary>
    /// Removes all whitespace from a string.
    /// </summary>
    /// <param name="value">The string to process</param>
    /// <returns>String with all whitespace removed</returns>
    public static string RemoveWhitespace(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Checks if a string contains a substring (case-insensitive).
    /// </summary>
    /// <param name="value">The string to search in</param>
    /// <param name="substring">The substring to search for</param>
    /// <returns>True if the substring is found (case-insensitive)</returns>
    /// <example>
    /// <code>
    /// if (errorMessage.ContainsIgnoreCase("not found"))
    /// {
    ///     // Handle not found error
    /// }
    /// </code>
    /// </example>
    public static bool ContainsIgnoreCase(this string value, string substring)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring))
        {
            return false;
        }

        return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Masks part of a sensitive string (e.g., for logging).
    /// Useful for logging device IDs or tokens without exposing full values.
    /// </summary>
    /// <param name="value">The string to mask</param>
    /// <param name="visibleCharacters">Number of characters to keep visible at start and end</param>
    /// <param name="maskCharacter">Character to use for masking</param>
    /// <returns>Masked string</returns>
    /// <example>
    /// <code>
    /// var masked = "ABC123DEF456".MaskSensitive(3);
    /// // Result: "ABC******456"
    /// </code>
    /// </example>
    public static string MaskSensitive(this string value, int visibleCharacters = 4, char maskCharacter = '*')
    {
        if (string.IsNullOrEmpty(value) || value.Length <= visibleCharacters * 2)
        {
            return new string(maskCharacter, value?.Length ?? 0);
        }

        var start = value[..visibleCharacters];
        var end = value[^visibleCharacters..];
        var masked = new string(maskCharacter, value.Length - (visibleCharacters * 2));

        return $"{start}{masked}{end}";
    }
}
