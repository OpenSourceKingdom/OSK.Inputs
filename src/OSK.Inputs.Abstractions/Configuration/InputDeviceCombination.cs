using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions.Configuration;

public readonly struct InputDeviceCombination(string name, InputDeviceIdentity[] deviceIdentities)
    : IEquatable<InputDeviceCombination>
{
    #region Api

    public string Name => name;

    public IReadOnlyCollection<InputDeviceIdentity> DeviceIdentities => deviceIdentities;

    public bool Contains(InputDeviceIdentity identity) 
        => deviceIdentities.Contains(identity);

    #endregion

    #region IEquatable

    public bool Equals(InputDeviceCombination other)
    {
        if (other.DeviceIdentities.Count != deviceIdentities.Length)
        {
            return false;
        }

        return deviceIdentities.SequenceEqual(other.DeviceIdentities);
    }

    #endregion
}
