using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;

public class InputScheme
{
    #region Constructors

    public InputScheme(string inputDefinitionName, string deviceName, string schemeName, bool isDefault, IEnumerable<InputActionMap> inputActions)
        : this(inputDefinitionName, new InputDeviceName(deviceName), schemeName, isDefault, inputActions)
    {

    }

    public InputScheme(string inputDefinitionName, InputDeviceName deviceName, string schemeName, bool isDefault, IEnumerable<InputActionMap> inputActions)
    {
        InputDefinitionName = inputDefinitionName;
        DeviceName = deviceName;
        SchemeName = schemeName;
        IsDefault = isDefault;
        InputActionMaps = inputActions.ToArray();
    }

    #endregion

    #region Variables

    public const string DefaultSchemeName = "Default";

    public string InputDefinitionName { get; }

    public InputDeviceName DeviceName { get; }

    public string SchemeName { get; }

    public bool IsDefault { get; }

    public IReadOnlyCollection<InputActionMap> InputActionMaps { get; }

    #endregion
}
