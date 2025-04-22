using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;
internal class InputDeviceActionBuilder<TInput>(string inputDefinitionName,
    string schemeName, IInputDeviceConfiguration deviceConfiguration) : IInputDeviceActionBuilder<TInput>
    where TInput: IInput
{
    #region Variables

    private readonly Dictionary<string, InputActionMap> _inputActionMapLookup = [];

    #endregion

    #region IInputDeviceActionBuilder

    public IInputDeviceActionBuilder<TInput> AssignInput(TInput input, InputPhase inputPhase, string actionKey)
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
                        return (Exception)new DuplicateNameException($"The input {group.Key} has already been added to the combination input {combinationInput.Name} for the input device {deviceConfiguration.DeviceName}");
                    }

                    return deviceConfiguration.IsValidInput(group.First())
                        ? null
                        : new InvalidOperationException($"Unable to add inputs of type {combinationInput.GetType().FullName} to the combonation input {combinationInput.Name} for the input device {deviceConfiguration.DeviceName} since it is not the expected input type.");
                }).FirstOrDefault(exceptionError => exceptionError is not null),
            _ => deviceConfiguration.IsValidInput(input)
                ? null
                : new InvalidOperationException($"Unable to add inputs of type {input.GetType().FullName} for the input device {deviceConfiguration.DeviceName} since it is not the expected input type.")
        };

        if (exception is not null)
        {
            throw exception;
        }

        var actionLookupKey = $"{actionKey}.{inputPhase}";
        if (_inputActionMapLookup.TryGetValue(actionLookupKey, out _))
        {
            throw new DuplicateNameException($"The input scheme {schemeName} for the device {deviceConfiguration.DeviceName} on input definition {inputDefinitionName} already has an input associated to the action key {actionKey} and input phase {inputPhase}.");
        }

        _inputActionMapLookup.Add(actionLookupKey, new InputActionMap(actionKey, input.Id, inputPhase));
        return this;
    }

    #endregion

    #region Helpers

    public InputDeviceActionMap Build()
    {
        return new InputDeviceActionMap(deviceConfiguration.DeviceName, _inputActionMapLookup.Values);
    }

    #endregion
}
