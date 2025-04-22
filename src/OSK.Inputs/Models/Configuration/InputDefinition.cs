using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Internal;

namespace OSK.Inputs.Models.Configuration;

public class InputDefinition(string name, IEnumerable<InputAction> inputActions, IEnumerable<InputScheme> inputSchemes)
{
    #region Variables

    public string Name => name;

    public IReadOnlyCollection<InputAction> InputActions => _actionLookup.Values;

    public IEnumerable<InputScheme> InputSchemes => _inputSchemeLookup.Values.SelectMany(schemesByController => schemesByController);

    private readonly Dictionary<string, InputScheme[]> _inputSchemeLookup = inputSchemes.GroupBy(inputScheme => inputScheme.ControllerId)
        .ToDictionary(inputControllerSchemeGroup => inputControllerSchemeGroup.Key, 
                      inputControllerSchemeGroup => inputControllerSchemeGroup.OrderBy(scheme => scheme.IsDefault).ThenBy(scheme => scheme.Name).ToArray());

    private readonly Dictionary<string, InputAction> _actionLookup = inputActions.ToDictionary(action => action.ActionKey);

    #endregion

    #region Helpers

    public InputAction this[string actionName] => _actionLookup[actionName];

    public InputDefinition Clone(IEnumerable<InputScheme>? additionalInputSchemes = null)
    {
        var inputSchemes = additionalInputSchemes is null 
            ? InputSchemes
            : InputSchemes.Concat(additionalInputSchemes).OrderByDescending(scheme => scheme is BuiltInInputScheme);
        return new InputDefinition(Name, InputActions, inputSchemes);
    }

    #endregion
}
