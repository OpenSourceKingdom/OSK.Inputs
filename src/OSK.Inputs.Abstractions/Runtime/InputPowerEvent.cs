using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Represents an input that provides a power to the input system. This most likely will either be either an Analog or Digital type of input.
/// </summary>
/// <param name="device">The device the input belongs to</param>
/// <param name="input">The input that triggered the event</param>
/// <param name="phase">The phase of the input</param>
/// <param name="inputIntensities">The collection of <see cref="InputIntensity"/> across all axes.</param>
public class InputPowerEvent(RuntimeDeviceIdentifier device, IDeviceInput input, InputPhase phase, InputIntensity[] inputIntensities)
    : DeviceInputEvent(device, input, phase)
{
    #region Variables

    public IEnumerable<InputIntensity> InputIntensities => inputIntensities;

    #endregion
}
