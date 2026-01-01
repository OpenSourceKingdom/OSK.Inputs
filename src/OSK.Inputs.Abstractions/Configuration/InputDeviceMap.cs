using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputDeviceMap
{
    #region Variables

    private readonly Dictionary<int, InputMap> _inputMapLookup = [];

    #endregion

    #region Api

    public required InputDeviceIdentity DeviceIdentity { get; init; }

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

    public InputMap? GetInputMap(int inputId)
        => _inputMapLookup.TryGetValue(inputId, out var map)
            ? map
            : null;

    #endregion
}
