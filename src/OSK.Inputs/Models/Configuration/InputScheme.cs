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
        ControllerName = deviceName;
        SchemeName = schemeName;
        IsDefault = isDefault;
        InputActionMaps = inputActions.ToArray();
    }

    #endregion

    public string InputDefinitionName { get; }

    public InputDeviceName ControllerName { get; }

    public string SchemeName { get; }

    public bool IsDefault { get; }

    public IReadOnlyCollection<InputActionMap> InputActionMaps { get; }
}
