using System;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// Represents an input on a device, like a joystick or button
/// </summary>
public abstract class DeviceInput(InputDeviceType deviceType, int id)
    : Input(deviceType), IEquatable<DeviceInput>
{    
    /// <summary>
    /// The unique id for the input
    /// </summary>
    public int Id => id;

    public bool Equals(DeviceInput other)
    {
        return other.DeviceType == DeviceType && other.Id == Id;
    }
}
