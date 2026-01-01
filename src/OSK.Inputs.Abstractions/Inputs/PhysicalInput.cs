using System;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Inputs;

public abstract class PhysicalInput(InputDeviceIdentity deviceIdentity, int id, string name, InputType inputType)
    : Input(id, name, inputType), IEquatable<PhysicalInput>
{
    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    public bool Equals(PhysicalInput other)
    {
        return other is not null && other.DeviceIdentity == DeviceIdentity && other.Id == Id;
    }
}
