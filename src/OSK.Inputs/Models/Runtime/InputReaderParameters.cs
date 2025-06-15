using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Models.Runtime;

/// <summary>
/// Initialization parameters for input readers that are created by the <see cref="IInputReaderProvider"/>
/// </summary>
/// <param name="deviceIdentifier">The identifier for the device</param>
/// <param name="inputs">The specific inputs that a reader should initialize and support for the current input system</param>
public class InputReaderParameters(InputDeviceIdentifier deviceIdentifier, IEnumerable<IInput> inputs)
{
    /// <summary>
    /// The identifier for the device that the input reader will be associated with
    /// </summary>
    public InputDeviceIdentifier DeviceIdentifier => deviceIdentifier;

    /// <summary>
    /// The specific inputs that the input reader should initialize and support
    /// </summary>
    public IEnumerable<IInput> Inputs => inputs;
}
