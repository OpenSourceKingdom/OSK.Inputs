using System;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// An input a user can interact with
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">A unique id for the input</param>
/// <param name="inputType">The type of input this represents</param>
public abstract class Input(string deviceType, int id, InputType inputType): IInput, IEquatable<Input>
{
    #region Variables

    /// <summary>
    /// The unique id for the input
    /// </summary>
    public int Id => id;

    /// <summary>
    /// The owner device type.
    /// 
    /// <br />
    /// Note: This may be empty
    /// </summary>
    public string DeviceType => deviceType;

    /// <summary>
    /// The specific type of input this represents
    /// </summary>
    public InputType InputType => inputType;

    #endregion

    #region IEquatable

    public bool Equals(Input other)
    {
        return other is not null && other.DeviceType.Equals(DeviceType, StringComparison.OrdinalIgnoreCase) && other.Id == Id;
    }

    #endregion
}
