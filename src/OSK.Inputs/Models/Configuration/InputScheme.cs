using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;

public class InputScheme(string inputDefinitionName, string controllerName, string schemeName, bool isDefault, IEnumerable<InputActionMap> inputActions)
{
    public string InputDefinitionName => inputDefinitionName;

    public string ControllerName => controllerName;

    public string SchemeName => schemeName;

    public bool IsDefault => isDefault;

    public IReadOnlyCollection<InputActionMap> InputActionMaps { get; } = inputActions.ToArray();
}
