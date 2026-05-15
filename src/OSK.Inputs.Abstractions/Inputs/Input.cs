namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// An input a user can interact with
/// </summary>
/// <param name="deviceType">The owner device type</param>
public abstract class Input(InputDeviceType deviceType): IInput
{
    #region Variables

    /// <summary>
    /// The type of devices this input belongs to.
    /// </summary>
    public InputDeviceType DeviceType => deviceType;

    #endregion
}
