using DeviceService.Domain.Common;
using DeviceService.Domain.Enums;

namespace DeviceService.Domain.Entities;

/// <summary>
/// Represents a physical device (gate, lift, counter, control).
/// </summary>
public class Device : BaseEntity
{
    /// <summary>
    /// Device name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Device type (Gate, Lift, Counter, Control).
    /// </summary>
    public DeviceType Type { get; private set; }

    /// <summary>
    /// Current operational status.
    /// </summary>
    public DeviceStatus Status { get; private set; }

    /// <summary>
    /// Physical location of the device.
    /// </summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>
    /// Last heartbeat timestamp (UTC).
    /// Device sends heartbeats periodically to indicate it's alive.
    /// </summary>
    public DateTime? LastHeartbeat { get; private set; }

    /// <summary>
    /// Firmware version running on the device.
    /// </summary>
    public string FirmwareVersion { get; private set; } = string.Empty;

    /// <summary>
    /// IP address of the device.
    /// </summary>
    public string IpAddress { get; private set; } = string.Empty;

    /// <summary>
    /// Additional metadata in JSON format.
    /// </summary>
    public string Metadata { get; private set; } = "{}";

    // Private parameterless constructor for EF Core
    private Device() { }

    /// <summary>
    /// Creates a new Device instance.
    /// </summary>
    /// <param name="name">Device name</param>
    /// <param name="type">Device type</param>
    /// <param name="location">Physical location</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="firmwareVersion">Firmware version</param>
    /// <returns>New Device instance</returns>
    public static Device Create(
        string name,
        DeviceType type,
        string location,
        string ipAddress,
        string firmwareVersion = "1.0.0")
    {
        return new Device
        {
            Name = name,
            Type = type,
            Status = DeviceStatus.Inactive,
            Location = location,
            IpAddress = ipAddress,
            FirmwareVersion = firmwareVersion,
            LastHeartbeat = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the device status.
    /// </summary>
    /// <param name="newStatus">New status</param>
    public void UpdateStatus(DeviceStatus newStatus)
    {
        Status = newStatus;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates device information.
    /// </summary>
    /// <param name="name">New name</param>
    /// <param name="location">New location</param>
    /// <param name="firmwareVersion">New firmware version</param>
    public void UpdateInfo(string? name = null, string? location = null, string? firmwareVersion = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (!string.IsNullOrWhiteSpace(location))
            Location = location;

        if (!string.IsNullOrWhiteSpace(firmwareVersion))
            FirmwareVersion = firmwareVersion;

        MarkAsUpdated();
    }

    /// <summary>
    /// Records a heartbeat from the device.
    /// </summary>
    public void RecordHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;

        // If device was offline, bring it back online
        if (Status == DeviceStatus.Offline)
        {
            Status = DeviceStatus.Active;
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if the device is considered online based on heartbeat.
    /// A device is online if it sent a heartbeat within the last 5 minutes.
    /// </summary>
    /// <returns>True if device is online, false otherwise</returns>
    public bool IsOnline()
    {
        if (!LastHeartbeat.HasValue)
            return false;

        return (DateTime.UtcNow - LastHeartbeat.Value).TotalMinutes < 5;
    }

    /// <summary>
    /// Marks the device as offline.
    /// </summary>
    public void MarkAsOffline()
    {
        Status = DeviceStatus.Offline;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the device metadata.
    /// </summary>
    /// <param name="metadata">JSON metadata</param>
    public void UpdateMetadata(string metadata)
    {
        Metadata = metadata;
        MarkAsUpdated();
    }
}
