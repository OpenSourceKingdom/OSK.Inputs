using System.Collections.Generic;

namespace OSK.Inputs.Abstractions.Configuration;

public readonly struct InputDeviceCombination(string name, InputDeviceIdentity[] deviceIdentities)
{
    public string Name => name;

    public IReadOnlyCollection<InputDeviceIdentity> DeviceIdentities => deviceIdentities;
}
