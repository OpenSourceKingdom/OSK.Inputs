using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Provides a mapping for a specific input scheme device and the related input definition actions
/// </summary>
/// <param name="deviceFamily">The device this map refers to</param>
/// <param name="actionMaps">The collections of action maps for the device inputs</param>
public class DeviceSchemeActionMap(InputDeviceFamily deviceFamily, IEnumerable<InputActionMap> actionMaps)
{
    #region Variables

    private readonly Dictionary<int, InputActionMap> _deviceInputMaps = actionMaps.Where(map => map.Input is DeviceInput)
                                                                                  .ToDictionary(inputMap => ((DeviceInput)inputMap.Input).Id);

    private readonly Dictionary<int, InputActionMap[]> _deviceVirtualInputLookup
        = actionMaps.Where(actionMap => actionMap.Input is VirtualInput)
                .SelectMany(map => ((VirtualInput)map.Input).GetLinkedInputs().OfType<DeviceInput>().Select(linkedInput => new { InputId = linkedInput.Id, ActionMap = map }))
                .GroupBy(inputLink => inputLink.InputId)
                .ToDictionary(inputVirtualGroup => inputVirtualGroup.Key, inputVirtualGroup => inputVirtualGroup.Select(ig => ig.ActionMap).ToArray());

    #endregion

    #region Api

    /// <summary>
    /// The device this map refers to
    /// </summary>
    public InputDeviceFamily DeviceFamily => deviceFamily;

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
