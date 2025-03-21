using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Models.Configuration;

public class InputDefinition(string name, IEnumerable<InputAction> inputActions, IEnumerable<InputScheme> inputSchemes)
{
    #region Variables

    public string Name => name;

    public IReadOnlyCollection<InputAction> InputActions { get; } = inputActions.ToArray();

    public IEnumerable<InputScheme> InputSchemes => _inputSchemeLookup.Values.SelectMany(schemesByController => schemesByController);

    private readonly Dictionary<string, InputScheme[]> _inputSchemeLookup = inputSchemes.GroupBy(inputScheme => inputScheme.ControllerName.Name)
        .ToDictionary(inputControllerSchemeGroup => inputControllerSchemeGroup.Key, 
                      inputControllerSchemeGroup => inputControllerSchemeGroup.OrderBy(scheme => scheme.IsDefault).ThenBy(scheme => scheme.SchemeName).ToArray());

    #endregion

    #region Helpers

    public IReadOnlyCollection<InputActionSchemeMap> GetInputActionSchemeMaps(InputControllerName controllerName, string schemeName)
    {
        if (!_inputSchemeLookup.TryGetValue(controllerName.Name, out var schemes))
        {
            return [];
        }

        var inputScheme = schemes.FirstOrDefaultByString(scheme => scheme.SchemeName, schemeName);
        if (inputScheme is null)
        {
            return [];
        }

        var inputActionLookup = InputActions.ToDictionary(action => action.ActionKey);

        List<InputActionSchemeMap> actionSchemeMaps = [];
        foreach (var inputMap in inputScheme.InputActionMaps)
        {
            actionSchemeMaps.Add(new InputActionSchemeMap(inputMap.InputKey, inputMap.InputPhase, inputActionLookup[inputMap.ActionKey].Options));
        }

        return actionSchemeMaps;
    }

    public InputDefinition Clone(IEnumerable<InputScheme>? additionalInputSchemes = null)
    {
        var inputSchemes = additionalInputSchemes is null 
            ? InputSchemes
            : InputSchemes.Concat(additionalInputSchemes);
        return new InputDefinition(Name, InputActions, inputSchemes);
    }

    #endregion
}
