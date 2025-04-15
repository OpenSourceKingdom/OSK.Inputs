using System;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;
public struct InputDeviceIdentifier(int deviceId, InputDeviceName deviceName)
{
    public int DeviceId => deviceId;

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
