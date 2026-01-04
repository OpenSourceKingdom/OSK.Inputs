namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// The various types of input that the input system can support
/// </summary>
public enum InputType
{
    /// <summary>
    /// Digital inputs provide only two different types of input: 0 or 1. 
    /// These inputs are most associated with buttons or similar types of physical input.
    /// </summary>
    Digital,

    /// <summary>
    /// Analog inputs provide a range of input, that is power, over a range like -1 to 1, or similar, where zero represents no power applied
    /// and 1 is full power. These inputs are most associated with joysticks, throttles, or other similar types of physical inputs.
    /// </summary>
    Analog,

    /// <summary>
    /// Represents a pointer on a device. This mostly associates with mice, touches, or other similar inputs that provide a pointer
    /// location on a device.
    /// </summary>
    Pointer
}
