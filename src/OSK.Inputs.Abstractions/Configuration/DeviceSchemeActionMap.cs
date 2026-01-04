using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Provides a mapping for a specific input scheme device and the related input definition actions
/// </summary>
/// <param name="deviceIdentity">The device this map refers to</param>
/// <param name="actionMaps">The collections of action maps for the device inputs</param>
public class DeviceSchemeActionMap(InputDeviceIdentity deviceIdentity, IEnumerable<InputActionMap> actionMaps)
{
    #region Variables

    private readonly Dictionary<int, InputActionMap> _deviceInputMaps = actionMaps.Where(map => map.Input is PhysicalInput)
                                                                                          .ToDictionary(inputMap => inputMap.InputId);

    private readonly Dictionary<int, InputActionMap[]> _deviceVirtualInputLookup
        = actionMaps.Where(map => map.Input is VirtualInput)
                .SelectMany(map => map.LinkedInputIds.Select(linkedId => new { InputId = linkedId, ActionMap = map }))
                .GroupBy(inputLink => inputLink.InputId)
                .ToDictionary(inputVirtualGroup => inputVirtualGroup.Key, inputVirtualGroup => inputVirtualGroup.Select(v => v.ActionMap).ToArray());

    #endregion

    #region Api

    /// <summary>
    /// The device this map refers to
    /// </summary>
    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    /// <summary>
    /// Gets the action maps for a specific input on the device
    /// </summary>
    /// <param name="id">The unique id for the input on the device</param>
    /// <returns>
    /// The list of action maps associated to the input with the given id, which can include virtual inputs, 
    /// or empty if the id is not for a configured input
    /// </returns>
    public IEnumerable<InputActionMap> GetActionMaps(int id)
    {
        if (_deviceVirtualInputLookup.TryGetValue(id, out var actionMaps))
        {
            foreach (var actionMap in actionMaps)
            {
                yield return actionMap;
            }
        }

        if (_deviceInputMaps.TryGetValue(id, out var physicalActionMap))
        {
            yield return physicalActionMap;
        }
    }

    #endregion
}
