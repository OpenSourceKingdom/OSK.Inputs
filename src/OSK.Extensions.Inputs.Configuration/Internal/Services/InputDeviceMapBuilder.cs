using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputDeviceMapBuilder(InputDeviceSpecification deviceSpecification): IInputDeviceMapBuilder
{
    #region Variables

    private readonly HashSet<int> _validInputIds = [.. deviceSpecification.GetInputs().Select(input => input.Id)];

    private readonly Dictionary<int, string> _inputMaps = [];

    #endregion

    #region IInputDeviceMapBuilder

    public IInputDeviceMapBuilder WithInputMap(int inputId, string actionName)
    {
        if (!_validInputIds.Contains(inputId))
        {
            throw new InvalidOperationException($"Unable to assign input {inputId} to a device map with {deviceSpecification.DeviceFamily} because it is not valid for the device.");
        }

        _inputMaps[inputId] = actionName;
        return this;
    }

    #endregion

    #region Helpers

    internal DeviceInputMap Build()
        => new()
        {
            DeviceFamily = deviceSpecification.DeviceFamily,
            InputMaps = [.. _inputMaps.Select(kvp => new InputMap() { InputId = kvp.Key, ActionName = kvp.Value })]
        };

    #endregion
}
