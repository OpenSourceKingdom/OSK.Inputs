using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputSystemConfigurationBuilder : IInputSystemConfigurationBuilder
{
    #region Variables

    private Dictionary<InputDeviceIdentity, InputDeviceSpecification> _deviceSpecifications = [];
    private Dictionary<string, Action<IInputDefinitionBuilder>> _definitionBuilderConfigurators = [];
    private Action<InputSystemJoinPolicyOptions>? _policyConfigurator;
    private Action<InputProcessingOptions>? _processorConfigurator;

    #endregion

    #region IInputSystemConfigurationBuilder

    public InputSystemConfiguration Build()
    {
        var policy = new InputSystemJoinPolicyOptions()
        {
            MaxLocalUsers = 1,
            DevicePairingBehavior = DevicePairingBehavior.Balanced,
            UserJoinBehavior = UserJoinBehavior.DeviceActivation
        };
        _policyConfigurator?.Invoke(policy);

        var processorOptions = new InputProcessingOptions()
        {
            TapReactivationTime = TimeSpan.FromSeconds(1),
            ActiveTimeThreshold = TimeSpan.FromSeconds(1)
        };
        _processorConfigurator?.Invoke(processorOptions);

        var definitions = _definitionBuilderConfigurators.Select(definitionKvp =>
        {
            var definitionBuilder = new InputDefinitionBuilder(definitionKvp.Key, this);
            definitionKvp.Value(definitionBuilder);

            return definitionBuilder.Build();
        });

        return new InputSystemConfiguration(_deviceSpecifications.Values, definitions,
            new InputProcessorConfiguration()
            {
                TapReactivationTime = processorOptions.TapReactivationTime,
                ActiveTimeThreshold = processorOptions.ActiveTimeThreshold,
            },
            new InputSystemJoinPolicy()
            {
                MaxUsers = policy.MaxLocalUsers,
                DeviceJoinBehavior = policy.DevicePairingBehavior,
                UserJoinBehavior = policy.UserJoinBehavior
            });
    }

    public IInputSystemConfigurationBuilder WithDevice(InputDeviceSpecification specification)
    {
        if (specification is null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        _deviceSpecifications[specification.DeviceIdentity] = specification;
        return this;
    }

    public IInputSystemConfigurationBuilder WithInputDefinition(string definitionName, Action<IInputDefinitionBuilder> definitionBuildConfigurator)
    {
        if (string.IsNullOrWhiteSpace(definitionName))
        {
            throw new ArgumentNullException(nameof(definitionName));
        }
        if (definitionBuildConfigurator is null)
        {
            throw new ArgumentNullException(nameof(definitionBuildConfigurator));
        }

        _definitionBuilderConfigurators[definitionName] = definitionBuildConfigurator;
        return this;
    }

    public IInputSystemConfigurationBuilder WithInputProcessing(Action<InputProcessingOptions> optionsConfigurator)
    {
        if (optionsConfigurator is null)
        {
            throw new ArgumentNullException(nameof(optionsConfigurator));
        }

        _processorConfigurator = optionsConfigurator;
        return this;
    }

    public IInputSystemConfigurationBuilder WithJoinPolicy(Action<InputSystemJoinPolicyOptions> optionsConfigurator)
    {
        if (optionsConfigurator is null)
        {
            throw new ArgumentNullException(nameof(optionsConfigurator));
        }

        _policyConfigurator = optionsConfigurator;
        return this;
    }

    #endregion
}
