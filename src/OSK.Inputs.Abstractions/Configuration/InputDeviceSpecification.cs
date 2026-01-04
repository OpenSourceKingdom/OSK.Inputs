using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Defines a specification for a given device, such as a keyboard, mouse, gamepad, or other input device
/// </summary>
public abstract class InputDeviceSpecification
{
    /// <summary>
    /// The specific device identity this specification refers to
    /// </summary>
    public abstract InputDeviceIdentity DeviceIdentity { get; }   

    /// <summary>
    /// The collection of <see cref="Input"/>s the device uses
    /// </summary>
    public abstract IReadOnlyCollection<Input> Inputs { get; }
}
