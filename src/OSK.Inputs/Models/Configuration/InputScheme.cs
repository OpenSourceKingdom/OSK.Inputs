using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;

public class InputScheme
{
    #region Constructors

    public InputScheme(string inputDefinitionName, string schemeName, bool isDefault, IEnumerable<InputDeviceActionMap> deviceActionMaps)
    {
        InputDefinitionName = inputDefinitionName;
        Name = schemeName;
        IsDefault = isDefault;
        DeviceActionMaps = deviceActionMaps.ToArray();
        ControllerId = string.Join(".", deviceActionMaps.Select(deviceActionMap => deviceActionMap.DeviceName));
    }

    #endregion

    #region Variables

    public const string DefaultSchemeName = "Default";

    public string ControllerId { get;}

    public string InputDefinitionName { get; }

    public IReadOnlyCollection<InputDeviceActionMap> DeviceActionMaps { get; }

    public string Name { get; }

    public bool IsDefault { get; }

    #endregion
}
