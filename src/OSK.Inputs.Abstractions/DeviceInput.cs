using System;

namespace OSK.Inputs.Abstractions;

public abstract class DeviceInput(InputDeviceIdentity deviceIdentity, int id, string name, InputType inputType)
    : Input(id, name, inputType), IEquatable<DeviceInput>
{
    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    public bool Equals(DeviceInput other)
    {
        return other is not null && other.DeviceIdentity == DeviceIdentity && other.Id == Id;
    }
}
