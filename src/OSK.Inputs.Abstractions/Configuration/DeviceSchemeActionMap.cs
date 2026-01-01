using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

public class DeviceSchemeActionMap(InputDeviceIdentity deviceIdentity, IEnumerable<DeviceInputActionMap> deviceActionMaps)
{
    #region Variables

    private readonly Dictionary<int, DeviceInputActionMap> _deviceInputMaps = deviceActionMaps.ToDictionary(inputMap => inputMap.InputId);

    private readonly Dictionary<int, int[]> _deviceVirtualInputLookup
        = deviceActionMaps
                .Where(map => map.Input is VirtualInput)
                .SelectMany(map => map.LinkedInputIds.Select(linkedId => new { InputId = linkedId, VirtualInputId = map.InputId }))
                .GroupBy(inputLink => inputLink.InputId)
                .ToDictionary(inputLinkGroup => inputLinkGroup.Key, inputLinkGroup => (int[])[.. inputLinkGroup.Select(link => link.VirtualInputId).Distinct()]);

    #endregion

    #region Api

    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    public IEnumerable<DeviceInputActionMap> GetActionMaps(int id)
    {
        if (!_deviceVirtualInputLookup.TryGetValue(id, out var linkedVirtualInputIds))
        {
            linkedVirtualInputIds = [];
        }

        var inputIdsToLookup = new HashSet<int>(linkedVirtualInputIds.Append(id));
        foreach (var inputId in inputIdsToLookup.Where(id => _deviceInputMaps.TryGetValue(id, out _)))
        {
            yield return _deviceInputMaps[inputId];
        }
    }

    #endregion
}
