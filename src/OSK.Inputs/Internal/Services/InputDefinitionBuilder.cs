using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputDefinitionBuilder(string definitionName, IEnumerable<IInputDeviceConfiguration> supportedDevices) : IInputDefinitionBuilder
{
    #region Variables

    private readonly Dictionary<string, InputAction> _actionLookup = [];
    private readonly Dictionary<string, Dictionary<string, InputScheme>> _controllerInputSchemeLookup = [];

    #endregion

    #region IInputDefinitionBuilder

    public IInputDefinitionBuilder AddAction(InputAction action)
    {
        if (string.IsNullOrWhiteSpace(action.ActionKey))
        {
            throw new ArgumentNullException(nameof(action));
        }
        if (action.ActionExecutor is null)
        {
            throw new InvalidOperationException($"Action {action.ActionKey} on definition {definitionName} did not have a valid action executor and is unusable.");
        }
        if (_actionLookup.TryGetValue(action.ActionKey, out _))
        {
            throw new DuplicateNameException($"An action has already been added to the input definition with the key {action.ActionKey}.");
        }

        _actionLookup.Add(action.ActionKey, action);
        return this;
    }

    public IInputDefinitionBuilder AddInputScheme(string schemeName, Action<IInputSchemeBuilder> buildAction)
    {
        if (string.IsNullOrWhiteSpace(schemeName))
        {
            throw new ArgumentException(nameof(schemeName));
        }
        if (buildAction is null)
        {
            throw new ArgumentNullException(nameof(buildAction));
        }

        var schemeBuilder = new InputSchemeBuilder(definitionName, supportedDevices, schemeName);
        buildAction(schemeBuilder);
        var scheme = schemeBuilder.Build();

        if (!_controllerInputSchemeLookup.TryGetValue(scheme.ControllerId, out var schemeLookup))
        {
            schemeLookup = [];
            _controllerInputSchemeLookup.Add(scheme.ControllerId, schemeLookup);
        }
        if (schemeLookup.TryGetValue(schemeName, out _))
        {
            throw new DuplicateNameException($"An input scheme has already been added to the input definition with the name {schemeName} for the {scheme.ControllerId} controller.");
        }

        schemeLookup.Add(schemeName, scheme);
        return this;
    }

    #endregion

    #region Helpers

    public InputDefinition Build()
    {
        return new InputDefinition(definitionName, _actionLookup.Values, 
            _controllerInputSchemeLookup.Values.SelectMany(schemeGroup => schemeGroup.Values));
    }

    #endregion
}
