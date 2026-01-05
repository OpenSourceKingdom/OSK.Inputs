using System;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// An input a user can interact with
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">A unique id for the input</param>
public abstract class Input(InputDeviceType deviceType, int id): IInput, IEquatable<IInput>
{
    #region Variables

    /// <summary>
    /// The unique id for the input
    /// </summary>
    public int Id => id;

    /// <summary>
    /// The type of devices this input belongs to.
    /// </summary>
    public InputDeviceType DeviceType => deviceType;

    #endregion

    #region IEquatable

    public bool Equals(IInput? other)
    {
        return other is not null && other.DeviceType == other.DeviceType && other.Id == Id;
    }

    #endregion
}
