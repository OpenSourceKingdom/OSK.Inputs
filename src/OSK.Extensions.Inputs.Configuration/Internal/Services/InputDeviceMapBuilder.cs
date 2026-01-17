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
    private readonly Dictionary<int, string?> _inputMaps = [];
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

        _inputMaps[inputId] = actionName;
        return this;
    }

    public IInputDeviceMapBuilder WithPassiveInput(int inputId)
    {
        if (!_validInputIds.Contains(inputId))
        {
            throw new InvalidOperationException($"Unable to assign input {inputId} to a device map with {deviceSpecification.DeviceFamily} because it is not valid for the device.");
        }

        _inputMaps[inputId] = null;

        return this;
    }

    public IInputDeviceMapBuilder WithVirtualInput(VirtualInput virtualInput, string actionName)
    {
        if (virtualInput is null)
        {
            throw new ArgumentNullException(nameof(virtualInput));
        }
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new InvalidOperationException($"Unable to assign virtual input to a device map with {deviceSpecification.DeviceFamily} because it was not specified as passive.");
        }

        _customVirtualInputs[actionName] = virtualInput;

        return this;
    }

    #endregion

    #region Helpers

    internal DeviceInputMap Build()
        => new()
        {
            DeviceFamily = deviceSpecification.DeviceFamily,
            InputMaps = [.. _inputMaps.Select(kvp => new InputMap() { InputId = kvp.Key, ActionName = kvp.Value })],
            VirtualMaps = [.. _customVirtualInputs.Select(kvp 
                => new VirtualInputMap() { ActionName = kvp.Key, Input = kvp.Value })]
        };

    #endregion
}
