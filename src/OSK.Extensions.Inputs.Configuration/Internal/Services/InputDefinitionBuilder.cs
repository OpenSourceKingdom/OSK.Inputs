using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputDefinitionBuilder(string name, IInputSystemConfigurationBuilder configurationBuilder) : IInputDefinitionBuilder
{
    #region Variables

    private readonly Dictionary<string, InputAction> _actions = [];
    private readonly Dictionary<string, Action<IInputSchemeBuilder>> _schemeBuilders = [];

    private bool _isDefault;

    #endregion

    #region IInputDefinitionBuilder

    public IInputDefinitionBuilder MakeDefault()
    {
        _isDefault = true;
        return this;
    }

    public IInputDefinitionBuilder WithAction(string name, Action<InputEventContext> executor, IEnumerable<InputPhase> triggerPhases, InputActionOptions actionOptions)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (executor is null)
        {
            throw new ArgumentNullException(nameof(executor));
        }
        if (triggerPhases is null)
        {
            throw new ArgumentNullException(nameof(triggerPhases));
        }
        if (actionOptions is null)
        {
            throw new ArgumentNullException(nameof(actionOptions));
        }

        _actions[name] = new InputAction(name, triggerPhases.ToHashSet(), executor,
            actionOptions.IncludePointerInformation, actionOptions.Description);
        return this;
    }

    public IInputDefinitionBuilder WithScheme(string name, Action<IInputSchemeBuilder> schemeBuildConfiguration)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (schemeBuildConfiguration is null)
        {
            throw new ArgumentNullException(nameof(schemeBuildConfiguration));
        }

        _schemeBuilders[name] = schemeBuildConfiguration;
        return this;
    }

    #endregion

    #region Helpers

    internal InputDefinition Build()
    {
        var schemes = _schemeBuilders.Select(schemeKvp =>
        {
            var schemeBuilder = new InputSchemeBuilder(schemeKvp.Key, configurationBuilder);
            schemeKvp.Value(schemeBuilder);

            return schemeBuilder.Build();
        });

        return new InputDefinition(name, _actions.Values, schemes, _isDefault);
    }

    #endregion
}
