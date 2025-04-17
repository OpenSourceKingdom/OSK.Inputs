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
    private readonly Dictionary<string, Dictionary<string, Action<IInputSchemeBuilder>>> _deviceSchemeBuilderLookup = [];

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

    public IInputDefinitionBuilder AddInputScheme(InputDeviceName deviceName, string schemeName, Action<IInputSchemeBuilder> buildAction)
    {
        if (string.IsNullOrWhiteSpace(deviceName.Name))
        {
            throw new ArgumentException(nameof(deviceName));
        }
        if (!supportedDevices.AnyByString(configuration => configuration.DeviceName.Name, deviceName.Name))
        {
            throw new InvalidOperationException($"Unable to add input scheme {schemeName} because the input system does not support the {deviceName} controller.");
        }
        if (string.IsNullOrWhiteSpace(schemeName))
        {
            throw new ArgumentException(nameof(schemeName));
        }
        if (buildAction is null)
        {
            throw new ArgumentNullException(nameof(buildAction));
        }
        if (!_deviceSchemeBuilderLookup.TryGetValue(deviceName.Name, out var schemeLookup))
        {
            schemeLookup = [];
            _deviceSchemeBuilderLookup.Add(deviceName.Name, schemeLookup);
        }
        if (schemeLookup.TryGetValue(schemeName, out _))
        {
            throw new DuplicateNameException($"An input scheme has already been added to the input definition with the name {schemeName} for the {deviceName} controller.");
        }

        schemeLookup.Add(schemeName, buildAction);
        return this;
    }

    #endregion

    #region Helpers

    public InputDefinition Build()
    {
        List<InputScheme> schemes = [];

        var schemeActions = _deviceSchemeBuilderLookup.SelectMany(deviceSchemeGroupLookup 
            => deviceSchemeGroupLookup.Value.Select(deviceSchemeAction 
                => new
                {
                    DeviceConfiguration = supportedDevices.FirstByString(deviceConfiguration => deviceConfiguration.DeviceName.Name, deviceSchemeGroupLookup.Key),
                    SchemeName = deviceSchemeAction.Key,
                    Action = deviceSchemeAction.Value
                }));

        foreach (var schemeActionData in schemeActions)
        {
            var schemeBuilder = new InputSchemeBuilder(definitionName, schemeActionData.DeviceConfiguration, schemeActionData.SchemeName);
            schemeActionData.Action(schemeBuilder);

            schemes.Add(schemeBuilder.Build());
        }

        return new InputDefinition(definitionName, _actionLookup.Values, schemes);
    }

    #endregion
}
