using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputScheme(string name, IEnumerable<DeviceInputMap> deviceMaps,
    bool isDefault, bool isCustom)
{
    #region Variables

    private readonly Dictionary<InputDeviceIdentity, DeviceInputMap> _deviceMapLookup
        = deviceMaps?.Where(map => map is not null).ToDictionary(map => map.DeviceIdentity) ?? [];

    #endregion

    #region Api

    /// <summary>
    /// A unique name for the scheme
    /// </summary>
    public string Name => name;

    /// <summary>
    /// Whether the scheme was created by a user
    /// </summary>
    public bool IsCustom => isCustom;

    /// <summary>
    /// Whether this scheme should be used before others, if no scheme has been selected by a user
    /// </summary>
    public bool IsDefault => isDefault;

    /// <summary>
    /// The collection of device input maps supported by this scheme
    /// </summary>
    public IReadOnlyCollection<DeviceInputMap> DeviceMaps => _deviceMapLookup.Values;

    /// <summary>
    /// Attempts to get a device map for a device identity
    /// </summary>
    /// <param name="deviceIdentity">The identity for a device to get maps for</param>
    /// <returns>The device map if one is configured, otherwise null</returns>
    public DeviceInputMap? GetDeviceMap(InputDeviceIdentity deviceIdentity)
        => _deviceMapLookup.TryGetValue(deviceIdentity, out var map)
            ? map
            : null;

    #endregion
}
