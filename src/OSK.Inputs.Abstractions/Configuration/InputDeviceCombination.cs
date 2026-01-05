using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Represents a set of input devices that can be used together when interacting with the system.
/// For example, this could be a Keyboard or a Keyboard and Mouse
/// </summary>
/// <param name="name">The display name for the combination</param>
/// <param name="deviceIdentities">The collection of devices the combination refers to</param>
public readonly struct InputDeviceCombination(string name, InputDeviceFamily[] deviceIdentities)
    : IEquatable<InputDeviceCombination>
{
    #region Api

    /// <summary>
    /// A name that represents the combination
    /// </summary>
    public string Name => name;

    /// <summary>
    /// The collection of devices the combination refers to
    /// </summary>
    public IReadOnlyCollection<InputDeviceFamily> DeviceIdentities => deviceIdentities;

    /// <summary>
    /// Determines if the provided device identity is in the combination
    /// </summary>
    /// <param name="identity">The identity to check the combination for</param>
    /// <returns>Whether this combination includes the device in question</returns>
    public bool Contains(InputDeviceFamily identity) 
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
