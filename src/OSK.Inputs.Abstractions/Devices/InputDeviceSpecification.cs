using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices;

/// <summary>
/// Defines a specification for a given device, such as a keyboard, mouse, gamepad, or other input device
/// </summary>
public abstract class InputDeviceSpecification
{
    #region Variables

    private Dictionary<int, IInput>? _inputLookup;

    #endregion

    #region Api

    /// <summary>
    /// The specific device identity this specification refers to
    /// </summary>
    public abstract InputDeviceFamily DeviceFamily { get; }

    /// <summary>
    /// Gets the collection of inputs for the device
    /// </summary>
    /// <returns>The collection of inputs</returns>
    public abstract IReadOnlyCollection<IInput> GetInputs();

    /// <summary>
    /// Tries to get the input with the given id
    /// </summary>
    /// <param name="inputId">The id to get</param>
    /// <param name="input">The input the id references, if it is a valid id</param>
    /// <returns>The input if the id is valid</returns>
    public bool TryGetInput(int inputId, out IInput input)
    {
        if (_inputLookup is null)
        {
            _inputLookup = GetInputs().ToDictionary(i => i.Id);
        }

        return _inputLookup.TryGetValue(inputId, out input);
    }

    #endregion
}
