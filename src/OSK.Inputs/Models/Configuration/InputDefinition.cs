using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

    public IReadOnlyCollection<InputScheme> GetInputSchemesByController(string controllerName)
        => _inputSchemeLookup.TryGetValue(controllerName, out var schemesByController) ? schemesByController : [];

    public InputDefinition Clone(IEnumerable<InputScheme>? additionalInputSchemes = null)
    {
        var inputSchemes = additionalInputSchemes is null 
            ? InputSchemes
            : InputSchemes.Concat(additionalInputSchemes);
        return new InputDefinition(Name, InputActions, inputSchemes);
    }

    #endregion
}
