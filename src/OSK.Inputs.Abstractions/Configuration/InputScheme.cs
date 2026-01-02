using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputScheme(string name, IEnumerable<InputDeviceMap> deviceMaps,
    bool isDefault, bool isCustom)
{
    #region Variables

    private readonly Dictionary<InputDeviceIdentity, InputDeviceMap> _deviceMapLookup
        = deviceMaps?.Where(map => map is not null).ToDictionary(map => map.DeviceIdentity) ?? [];

    #endregion

    #region Api

    public string Name => name;

    public bool IsCustom => isCustom;

    public bool IsDefault => isDefault;

    public IReadOnlyCollection<InputDeviceMap> DeviceMaps => _deviceMapLookup.Values;

    public InputDeviceMap? GetDeviceMap(InputDeviceIdentity deviceIdentity)
        => _deviceMapLookup.TryGetValue(deviceIdentity, out var map)
            ? map
            : null;

    #endregion
}
