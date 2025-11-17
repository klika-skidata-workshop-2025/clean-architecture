namespace DeviceService.Domain.Enums;

/// <summary>
/// Represents the type of physical device.
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Gate device (e.g., entry/exit gate).
    /// </summary>
    Gate = 1,

    /// <summary>
    /// Lift device (e.g., ski lift, chairlift).
    /// </summary>
    Lift = 2,

    /// <summary>
    /// Counter device (e.g., people counter).
    /// </summary>
    Counter = 3,

    /// <summary>
    /// Control device (e.g., control panel, terminal).
    /// </summary>
    Control = 4
}
