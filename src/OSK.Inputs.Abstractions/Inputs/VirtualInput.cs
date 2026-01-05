using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// A virtual input is a type of input that is triggered on behalf of the user given some set of rules or conditions.
/// For example, combinations, gestures, or other inputs are virtual as there is no hardware mechanism to trigger them.
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">The unique id for the input</param>
public abstract class VirtualInput(InputDeviceType deviceType, int id)
    : Input(deviceType, id)
{
    /// <summary>
    /// Determines if the virtual input possesses the given input as part of its rules
    /// </summary>
    /// <param name="input">The input to check</param>
    /// <returns>Whether this virtual input uses the given input</returns>
    public abstract bool Contains(IInput input);
}
