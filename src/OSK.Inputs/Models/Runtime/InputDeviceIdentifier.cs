using System;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;

/// <summary>
/// A struct that represents a specific input device
/// </summary>
/// <param name="deviceId">The device's input system id</param>
/// <param name="deviceName">The name of the input device</param>
public struct InputDeviceIdentifier(int deviceId, InputDeviceName deviceName)
{
    /// <summary>
    /// The input system's unique id for the device
    /// </summary>
    public int DeviceId => deviceId;

    /// <summary>
    /// The name of the input device
    /// </summary>
    public InputDeviceName DeviceName => deviceName;

    #region Operators

    public override bool Equals(object? obj)
    {
        return obj is InputDeviceIdentifier identifier &&
               DeviceId == identifier.DeviceId &&
               DeviceName == identifier.DeviceName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DeviceId, DeviceName);
    }

    public static bool operator ==(InputDeviceIdentifier left, InputDeviceIdentifier right)
    {
        return left.DeviceId == right.DeviceId && left.DeviceName == right.DeviceName;
    }

    public static bool operator !=(InputDeviceIdentifier left, InputDeviceIdentifier right)
    {
        return left.DeviceId != right.DeviceId || left.DeviceName != right.DeviceName;
    }

    #endregion
}
