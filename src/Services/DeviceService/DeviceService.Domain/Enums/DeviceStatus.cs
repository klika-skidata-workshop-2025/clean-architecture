namespace DeviceService.Domain.Enums;

/// <summary>
/// Represents the operational status of a device.
/// </summary>
public enum DeviceStatus
{
    /// <summary>
    /// Device is active and operational.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Device is inactive or idle.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Device is undergoing maintenance.
    /// </summary>
    Maintenance = 3,

    /// <summary>
    /// Device is blocked and not operational.
    /// </summary>
    Blocked = 4,

    /// <summary>
    /// Device is offline or unreachable.
    /// </summary>
    Offline = 5,

    /// <summary>
    /// Device is in an error state.
    /// </summary>
    Error = 6
}
