namespace OSK.Inputs.Abstractions.Devices;

/// <summary>
/// The type of device an input device is
/// </summary>
public enum InputDeviceType
{
    /// <summary>
    /// A generic input device, not specific to any particular device type
    /// </summary>
    Generic = 0,

    /// <summary>
    /// The device is a keyboard
    /// </summary>
    Keyboard = 1,

    /// <summary>
    /// The device is a mouse
    /// </summary>
    Mice = 2,

    /// <summary>
    /// The device is a gamepad
    /// </summary>
    GamePad = 3,
}
