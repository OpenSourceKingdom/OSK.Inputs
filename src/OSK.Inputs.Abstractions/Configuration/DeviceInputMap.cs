using System.Collections.Generic;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// A map for actions and inputs for a given input device
/// </summary>
public class DeviceInputMap
{
    #region Variables

    private readonly Dictionary<int, InputMap> _inputMapLookup = [];

    #endregion

    #region Api

    /// <summary>
    /// The device this device map is for
    /// </summary>
    public required InputDeviceFamily DeviceFamily { get; init; }

    /// <summary>
    /// The collection of input maps this device map supports
    /// </summary>
    public required IReadOnlyCollection<InputMap> InputMaps 
    {
        get => _inputMapLookup.Values;
        init
        {
            foreach (var inputMap in value)
            {
                _inputMapLookup[inputMap.InputId] = inputMap;
            }
        } 
    }

    /// <summary>
    /// Attempts to get an input map for a given input
    /// </summary>
    /// <param name="inputId">The unique id for the input on the device</param>
    /// <returns>The map for the input if is a configured input, otherwise null</returns>
    public InputMap? GetInputMap(int inputId)
        => _inputMapLookup.TryGetValue(inputId, out var map)
            ? map
            : null;

    #endregion
}
