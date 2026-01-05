using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices;

/// <summary>
/// Defines a specification for a given device, such as a keyboard, mouse, gamepad, or other input device
/// </summary>
public abstract class InputDeviceSpecification
{
    /// <summary>
    /// The specific device identity this specification refers to
    /// </summary>
    public abstract InputDeviceFamily DeviceFamily { get; }


    /// <summary>
    /// Gets the collection of inputs for the device
    /// </summary>
    /// <returns>The collection of inputs</returns>
    public abstract IReadOnlyCollection<IInput> GetInputs();
}
