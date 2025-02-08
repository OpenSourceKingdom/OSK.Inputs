using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;

public class InputDefinition(string name, bool allowCustomInputSchemes,
    IEnumerable<InputControllerConfiguration> supportedInputControllers, IEnumerable<InputAction> inputActions)
{
    #region Variables

    public string Name => name;

    public bool AllowCustomInputSchemes => allowCustomInputSchemes;

    public IReadOnlyCollection<InputAction> InputActions { get; } = inputActions.ToArray();

    public IReadOnlyCollection<InputControllerConfiguration> SupportedInputControllers { get; } = supportedInputControllers.ToArray();

    #endregion

    #region Helpers

    public InputDefinition Clone(IEnumerable<InputScheme>? additionalInputSchemes = null)
    {
        var additionalInputSchemeLookup = additionalInputSchemes?.GroupBy(inputScheme => inputScheme.ControllerName,
            StringComparer.Ordinal).ToDictionary(group => group.Key) ?? [];

        List<InputControllerConfiguration> inputControllers = [];
        foreach (var inputController in SupportedInputControllers)
        {
            additionalInputSchemeLookup.TryGetValue(inputController.ControllerName, out var additionalSchemes);

            inputControllers.Add(inputController.Clone(additionalInputSchemes: additionalSchemes));
        }

        return new InputDefinition(Name, AllowCustomInputSchemes, inputControllers, InputActions);
    }

    #endregion
}
