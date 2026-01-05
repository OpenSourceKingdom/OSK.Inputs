using System;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

public interface IInput: IEquatable<IInput>
{
    public int Id { get; }

    InputDeviceType DeviceType { get; }
}
