using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputDefinitionBuilder(string definitionName, IEnumerable<IInputControllerConfiguration> supportedControllers) : IInputDefinitionBuilder
{
    #region Variables

    private readonly Dictionary<string, InputAction> _actionLookup = [];
    private readonly Dictionary<string, Dictionary<string, Action<IInputSchemeBuilder>>> _controllerSchemeBuilderLookup = [];

    #endregion

    #region IInputDefinitionBuilder

    public IInputDefinitionBuilder AddAction(InputAction action)
    {
        if (string.IsNullOrWhiteSpace(action.ActionKey))
        {
            throw new ArgumentNullException(nameof(action));
        }
        if (_actionLookup.TryGetValue(action.ActionKey, out _))
        {
            throw new DuplicateNameException($"An action has already been added to the input definition with the key {action.ActionKey}.");
        }

        _actionLookup.Add(action.ActionKey, action);
        return this;
    }

    public IInputDefinitionBuilder AddInputScheme(string controllerName, string schemeName, Action<IInputSchemeBuilder> buildAction)
    {
        if (string.IsNullOrWhiteSpace(controllerName))
        {
            throw new ArgumentException(nameof(controllerName));
        }
        if (!supportedControllers.AnyByString(configuration => configuration.ControllerName, controllerName))
        {
            throw new InvalidOperationException($"Unable to add input scheme {schemeName} because the input system does not support the {controllerName} controller.");
        }
        if (string.IsNullOrWhiteSpace(schemeName))
        {
            throw new ArgumentException(nameof(schemeName));
        }
        if (buildAction is null)
        {
            throw new ArgumentNullException(nameof(buildAction));
        }
        if (!_controllerSchemeBuilderLookup.TryGetValue(controllerName, out var schemeLookup))
        {
            schemeLookup = [];
            _controllerSchemeBuilderLookup.Add(controllerName, schemeLookup);
        }
        if (schemeLookup.TryGetValue(schemeName, out _))
        {
            throw new DuplicateNameException($"An input scheme has already been added to the input definition with the name {schemeName} for the {controllerName} controller.");
        }

        schemeLookup.Add(schemeName, buildAction);
        return this;
    }

    #endregion

    #region Helpers

    public InputDefinition Build()
    {
        List<InputScheme> schemes = [];

        var schemeActions = _controllerSchemeBuilderLookup.SelectMany(controllerSchemeGroupLookup 
            => controllerSchemeGroupLookup.Value.Select(controllerSchemeAction 
                => new
                {
                    ControllerConfiguration = supportedControllers.FirstByString(controllerConfiguraiton => controllerConfiguraiton.ControllerName, controllerSchemeGroupLookup.Key),
                    SchemeName = controllerSchemeAction.Key,
                    Action = controllerSchemeAction.Value
                }));

        foreach (var schemeActionData in schemeActions)
        {
            var schemeBuilder = new InputSchemeBuilder(definitionName, schemeActionData.ControllerConfiguration, schemeActionData.SchemeName);
            schemeActionData.Action(schemeBuilder);

            schemes.Add(schemeBuilder.Build());
        }

        return new InputDefinition(definitionName, _actionLookup.Values, schemes);
    }

    #endregion
}
