using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputSchemeBuilder(string inputDefinitionName, IInputControllerConfiguration controllerConfiguration, string schemeName) 
    : IInputSchemeBuilder
{
    #region Variables

    private bool _isDefault;
    private readonly Dictionary<string, InputActionMap> _inputActionMapLookup = [];

    #endregion

    #region IInputSchemeBuilder

    public IInputSchemeBuilder AssignInput(string actionKey, IInput input, InputPhase inputPhase)
    {
        if (string.IsNullOrWhiteSpace(actionKey))
        {
            throw new ArgumentNullException(nameof(actionKey));
        }
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        Exception? exception = input switch
        {
            CombinationInput combinationInput => combinationInput.Inputs.GroupBy(comboInput => comboInput.Name)
                .Select(group =>
                {
                    if (group.Count() > 1)
                    {
                        return (Exception) new DuplicateNameException($"The input {group.Key} has already been added to the combination input {combinationInput.Name} for the input controller {controllerConfiguration.ControllerName}");
                    }

                    return controllerConfiguration.IsValidInput(group.First())
                        ? null
                        : new InvalidOperationException($"Unable to add inputs of type {combinationInput.GetType().FullName} to the combonation input {combinationInput.Name} for the input controller {controllerConfiguration.ControllerName} since it is not the expected input type.");
                }).FirstOrDefault(exceptionError => exceptionError is not null),
            _ => controllerConfiguration.IsValidInput(input)
                ? null
                : new InvalidOperationException($"Unable to add inputs of type {input.GetType().FullName} for the input controller {controllerConfiguration.ControllerName} since it is not the expected input type.")
        };

        if (exception is not null) 
        {
            throw exception;
        }
        if (_inputActionMapLookup.TryGetValue(actionKey, out _))
        {
            throw new DuplicateNameException($"The input scheme {schemeName} for the controller {controllerConfiguration.ControllerName} using input definition {inputDefinitionName} already has an input associated to the action key {actionKey}.");
        }

        _inputActionMapLookup.Add(actionKey, new InputActionMap(actionKey, input.Name, inputPhase));
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
        return new BuiltInInputScheme(inputDefinitionName, controllerConfiguration.ControllerName, schemeName, _isDefault, _inputActionMapLookup.Values);
    }

    #endregion
}
