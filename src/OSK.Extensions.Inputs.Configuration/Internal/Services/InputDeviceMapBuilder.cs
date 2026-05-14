using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputDeviceMapBuilder(InputDeviceSpecification deviceSpecification): IInputDeviceMapBuilder
{
    #region Variables

    private readonly HashSet<int> _validInputIds = [.. deviceSpecification.GetInputs().Select(input => input.Id)];
    private readonly Dictionary<int, (string? ActionName, InputStream?)> _inputMaps = [];
    private readonly Dictionary<string, VirtualInput> _customVirtualInputs = [];

    #endregion

    #region IInputDeviceMapBuilder

    public IInputDeviceMapBuilder WithInputMap(int inputId, string actionName)
    {
        if (!_validInputIds.Contains(inputId))
        {
            throw new InvalidOperationException($"Unable to assign input {inputId} to a device map with {deviceSpecification.DeviceFamily} because it is not valid for the device.");
        }
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new InvalidOperationException($"Unable to assign input {inputId} to a device map with {deviceSpecification.DeviceFamily} because the action map was null and the input wasn't passive.");
        }

        _inputMaps[inputId] = (actionName, null);
        return this;
    }

    public IInputDeviceMapBuilder WithInputStream<TInputStream>(int inputId, string? actionName = default)
        where TInputStream: InputStream
    {
        if (!_validInputIds.Contains(inputId))
        {
            throw new InvalidOperationException($"Unable to assign input {inputId} to a device map with {deviceSpecification.DeviceFamily} because it is not valid for the device.");
        }

        _inputMaps[inputId] = (actionName, (InputStream)Activator.CreateInstance(typeof(TInputStream), [deviceSpecification.DeviceFamily.DeviceType, inputId]));

        return this;
    }

    public IInputDeviceMapBuilder WithVirtualInput<TVirtualInput>(int[] inputIds, string actionName)
        where TVirtualInput : VirtualInput
    {
        if (inputIds is null)
        {
            throw new ArgumentNullException(nameof(inputIds));
        }
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new InvalidOperationException($"Unable to assign virtual input to a device map with {deviceSpecification.DeviceFamily} because it was not specified as passive.");
        }

        var inputs = deviceSpecification.GetInputs().Where(input => inputIds.Contains(input.Id)).ToArray();
        if (inputs.Length != inputIds.Length)
        {
            var invalidInputIds = inputIds.Except(inputs.Select(i => i.Id));
            throw new InvalidOperationException($"Unable to assign virtual input to a device map with {deviceSpecification.DeviceFamily} because the following input ids are not valid for the device: {string.Join(", ", invalidInputIds)}.");
        }

        _customVirtualInputs[actionName] = (TVirtualInput)Activator.CreateInstance(typeof(TVirtualInput), [deviceSpecification.DeviceFamily.DeviceType, inputs]);

        return this;
    }

    #endregion

    #region Helpers

    internal DeviceInputMap Build()
        => new()
        {
            DeviceFamily = deviceSpecification.DeviceFamily,
            InputMaps = [.. _inputMaps.Select(kvp => new InputMap() { InputId = kvp.Key, ActionName = kvp.Value.ActionName })],
            VirtualMaps = [.. _customVirtualInputs.Select(kvp 
                => new VirtualInputMap() { ActionName = kvp.Key, Input = kvp.Value })]
        };

    #endregion
}
