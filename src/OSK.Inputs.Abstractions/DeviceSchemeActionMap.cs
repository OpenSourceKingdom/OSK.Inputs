using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions;

public class DeviceSchemeActionMap(InputDeviceIdentity deviceIdentity, IEnumerable<InputActionMap> actionMaps)
{
    #region Variables

    private readonly Dictionary<int, DeviceInput> _deviceInputs = 
        actionMaps.Select(actionMap => actionMap.Input).SelectMany(input =>
        {
            return input switch
            {
                DeviceInput deviceInput => [ deviceInput ],
                CombinationInput combinationInput => combinationInput.DeviceInputs,
                _ => []
            };
        })
        .GroupBy(input => input.Id)
        .ToDictionary(inputGroup => inputGroup.Key, inputGroup => inputGroup.First());

    private readonly Dictionary<int, InputActionMap> _inputMaps = actionMaps.ToDictionary(inputMap => inputMap.InputId);

    #endregion

    #region Api

    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    public InputActionMap? GetActionMap(int id)
        => _inputMaps.TryGetValue(id, out var map)
         ? map
         : null;

    #endregion
}
