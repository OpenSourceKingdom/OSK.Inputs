using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;

public class InputScheme
{
    #region Constructors

    public InputScheme(string inputDefinitionName, string controllerName, string schemeName, bool isDefault, IEnumerable<InputActionMap> inputActions)
        : this(inputDefinitionName, new InputControllerName(controllerName), schemeName, isDefault, inputActions)
    {

    }

    public InputScheme(string inputDefinitionName, InputControllerName controllerName, string schemeName, bool isDefault, IEnumerable<InputActionMap> inputActions)
    {
        InputDefinitionName = inputDefinitionName;
        ControllerName = controllerName;
        SchemeName = schemeName;
        IsDefault = isDefault;
        InputActionMaps = inputActions.ToArray();
    }

    #endregion

    public string InputDefinitionName { get; }

    public InputControllerName ControllerName { get; }

    public string SchemeName { get; }

    public bool IsDefault { get; }

    public IReadOnlyCollection<InputActionMap> InputActionMaps { get; }
}
