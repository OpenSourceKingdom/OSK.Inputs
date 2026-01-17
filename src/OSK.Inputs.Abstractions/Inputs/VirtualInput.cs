using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// A virtual input is a type of input that is triggered on behalf of the user given some set of rules or conditions.
/// For example, combinations, gestures, or other inputs are virtual as there is no hardware mechanism to trigger them.
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="inputs">The list of inputs associated with this virtual input</param>
public abstract class VirtualInput(InputDeviceType deviceType, IEnumerable<IInput> inputs)
    : Input(deviceType)
{
    #region Variables

    private readonly Dictionary<InputDeviceType, DeviceInput[]> _deviceInputLookup = inputs.OfType<DeviceInput>().GroupBy(d => d.DeviceType)
                                                                                           .ToDictionary(inputGroup => inputGroup.Key, inputGroup => inputGroup.ToArray());

    #endregion

    #region Api

    /// <summary>
    /// Gets the collection of inputs this virtual input references
    /// </summary>
    /// <returns>The collection of inputs</returns>
    public IEnumerable<IInput> GetLinkedInputs()
        => _deviceInputLookup.SelectMany(inputLookup => inputLookup.Value);

    /// <summary>
    /// Determines if the virtual input possesses the given input as part of its rules
    /// </summary>
    /// <param name="input">The input to check</param>
    /// <returns>Whether this virtual input uses the given input</returns>
    public bool Contains(IInput input)
        => input switch
        {
            DeviceInput deviceInput => _deviceInputLookup.TryGetValue(deviceInput.DeviceType, out var deviceInputs) && deviceInputs.Contains(deviceInput),
            _ => false
        };

    #endregion
}
