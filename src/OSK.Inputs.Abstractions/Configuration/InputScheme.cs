using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputScheme(string name, IEnumerable<VirtualInput> virtualInputs, IEnumerable<InputDeviceMap> deviceMaps,
    bool isDefault, bool isCustom)
{
    #region Variables

    private readonly Dictionary<InputDeviceIdentity, InputDeviceMap> _deviceMapLookup = deviceMaps.ToDictionary(map => map.DeviceIdentity);

    #endregion

    #region Api

    public string Name => name;

    public bool IsCustom => isCustom;

    public bool IsDefault => isDefault;

    public IReadOnlyCollection<VirtualInput> VirtualInputs { get; } = [.. virtualInputs];

    public IReadOnlyCollection<InputDeviceMap> DeviceMaps => _deviceMapLookup.Values;

    public InputDeviceMap? GetDeviceMap(InputDeviceIdentity deviceIdentity)
        => _deviceMapLookup.TryGetValue(deviceIdentity, out var map)
            ? map
            : null;

    #endregion
}
