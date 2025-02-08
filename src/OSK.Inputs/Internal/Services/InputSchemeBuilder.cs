using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputSchemeBuilder(string inputDefinitionName, string controllerName, string schemeName,
    IEnumerable<IInputReceiverDescriptor> receiverDescriptions) 
    : IInputSchemeBuilder
{
    #region Variables

    private bool _isDefault;
    private readonly Dictionary<string, IInputReceiverDescriptor> _receiverDescriptionLookup = receiverDescriptions.ToDictionary(receiver => receiver.ReceiverName);
    private readonly Dictionary<string, Dictionary<string, InputActionMap>> _inputConfigurations = [];

    #endregion

    #region IInputSchemeBuilder

    public IInputSchemeBuilder AssignInput(string receiverName, string actionKey, IInput input, InputPhase inputPhase)
    {
        if (string.IsNullOrWhiteSpace(receiverName))
        {
            throw new ArgumentNullException(nameof(receiverName));
        }
        if (string.IsNullOrWhiteSpace(actionKey))
        {
            throw new ArgumentNullException(nameof(actionKey));
        }
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (!_receiverDescriptionLookup.TryGetValue(receiverName, out var receiverDescription))
        {
            throw new InvalidOperationException($"The controller scheme {schemeName} for {controllerName}, {inputDefinitionName} is not able to support the input receiver {receiverName}");
        }

        var inputValidationError = input switch
        {
            CombinationInput combinationInput => combinationInput.Inputs.GroupBy(comboInput => comboInput.Name)
                .Select(group =>
                {
                    if (group.Count() > 1)
                    {
                        return $"The input {group.Key} has already been added to the combination input {combinationInput.Name} for the input receiver {receiverDescription.ReceiverName}";
                    }

                    return receiverDescription.IsValidInput(group.First())
                        ? null
                        : $"Unable to add inputs of type {combinationInput.GetType().FullName} to the combonation input {combinationInput.Name} for the input receiver {receiverDescription.ReceiverName} since it is not the expected input type.";
                }).FirstOrDefault(error => !string.IsNullOrWhiteSpace(error)),
            _ => receiverDescription.IsValidInput(input) 
                ? null 
                : $"Unable to add inputs of type {input.GetType().FullName} for the input receiver {receiverDescription.ReceiverName} since it is not the expected input type."
        };

        if (!string.IsNullOrWhiteSpace(inputValidationError)) 
        {
            throw new InvalidOperationException(inputValidationError);
        }
        if (!_inputConfigurations.TryGetValue(receiverName, out var receiverInputConfiguration))
        {
            receiverInputConfiguration = [];
            _inputConfigurations.Add(receiverName, receiverInputConfiguration);
        }

        if (receiverInputConfiguration.TryGetValue(actionKey, out _))
        {
            throw new InvalidOperationException($"The input scheme {schemeName} for the receiver {receiverName} on controller {controllerName} using input definition {inputDefinitionName} already has an input associated to the action key {actionKey}.");
        }

        receiverInputConfiguration.Add(actionKey, new InputActionMap(actionKey, input.Name, inputPhase));
        return this;
    }

    public IInputSchemeBuilder MakeDefault()
    {
        _isDefault = true;
        return this;
    }

    #endregion

    #region Helpers

    public InputScheme Build()
    {
        var inputConfigurations = _inputConfigurations.Select(configuration =>
        {
            return new InputReceiverConfiguration(configuration.Key, configuration.Value.Values);
        });
        return new BuiltInInputScheme(inputDefinitionName, controllerName, schemeName, inputConfigurations, _isDefault);
    }

    #endregion
}
