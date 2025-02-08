using System;
using System.Collections.Generic;
using System.Data;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;
internal class InputControllerBuilder(string definitionName, string controllerName) : IInputControllerBuilder
{
    #region Variables

    private readonly Dictionary<string, IInputReceiverDescriptor> _receiverDescriptions = [];
    private readonly Dictionary<string, Action<IInputSchemeBuilder>> _inputSchemeConfigurations = [];

    #endregion

    #region IInputControllerBuilder

    public IInputControllerBuilder AddInputReceiver(IInputReceiverDescriptor descriptor)
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }
        if (string.IsNullOrWhiteSpace(descriptor.ReceiverName))
        {
            throw new ArgumentNullException(nameof(descriptor.ReceiverName));
        }
        if (descriptor.InputReceiverType is null)
        {
            throw new ArgumentNullException(nameof(descriptor.InputReceiverType));
        }
        if (!typeof(IInputReceiver).IsAssignableFrom(descriptor.InputReceiverType))
        {
            throw new InvalidOperationException($"The input receiver type provdied {descriptor.InputReceiverType.FullName} is not of type {typeof(IInputReceiver).FullName}");
        }

        if (_receiverDescriptions.TryGetValue(descriptor.ReceiverName, out _))
        {
            throw new DuplicateNameException($"An input receiver with the name {descriptor.ReceiverName} has already been added to the input definition.");
        }

        _receiverDescriptions.Add(descriptor.ReceiverName, descriptor);
        return this;
    }

    public IInputControllerBuilder AddInputScheme(string schemeName, Action<IInputSchemeBuilder> buildConfiguration)
    {
        if (string.IsNullOrWhiteSpace(schemeName))
        {
            throw new ArgumentNullException(nameof(schemeName));
        }
        if (buildConfiguration is null)
        {
            throw new ArgumentNullException(nameof(buildConfiguration));
        }

        if (_inputSchemeConfigurations.TryGetValue(schemeName, out _))
        {
            throw new DuplicateNameException($"An input scheme configuration with the name {schemeName} has already been added to the input definition.");
        }

        _inputSchemeConfigurations.Add(schemeName, buildConfiguration);
        return this;
    }

    #endregion

    #region Helpers

    internal InputControllerConfiguration Build()
    {
        List<InputScheme> schemes = [];
        foreach (var schemeConfiguration in _inputSchemeConfigurations)
        {
            var schemeBuilder = new InputSchemeBuilder(definitionName, controllerName, schemeConfiguration.Key,
                _receiverDescriptions.Values);

            schemeConfiguration.Value(schemeBuilder);
            var scheme = schemeBuilder.Build();
            schemes.Add(scheme);
        }

        return new InputControllerConfiguration(controllerName, _receiverDescriptions.Values, schemes);
    }

    #endregion
}
