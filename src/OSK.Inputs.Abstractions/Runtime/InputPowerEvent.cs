using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Represents an input that provides a power to the input system. This most likely will either be either an Analog or Digital type of input.
/// </summary>
/// <param name="device">The device the input belongs to</param>
/// <param name="inputId">The input id that triggered the event</param>
/// <param name="phase">The phase of the input</param>
/// <param name="inputIntensities">The collection of <see cref="InputIntensity"/> across all axes.</param>
public class InputPowerEvent(RuntimeDeviceIdentifier device, int inputId, InputPhase phase, InputIntensity[] inputIntensities)
    : DeviceInputEvent(device, inputId, phase)
{
    #region Variables

    public IEnumerable<InputIntensity> InputIntensities => inputIntensities;

    #endregion
}
