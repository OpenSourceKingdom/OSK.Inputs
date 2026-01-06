using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputDeviceMapBuilder<TDeviceSpecification, TInput>(InputDeviceFamily deviceFamily) 
    : IInputDeviceMapBuilder<TDeviceSpecification, TInput>
    where TInput: Enum
    where TDeviceSpecification: InputDeviceSpecification<TInput>, new()
{
    #region Variables

    private readonly Dictionary<int, string> _inputMaps = [];

    #endregion

    #region IInputDeviceMapBuilder

    public IInputDeviceMapBuilder<TDeviceSpecification, TInput> WithInputMap(TInput input, string actionName)
    {
        _inputMaps[Convert.ToInt32(input)] = actionName;
        return this;
    }

    #endregion

    #region Helpers

    internal DeviceInputMap Build()
        => new()
        {
            DeviceFamily = deviceFamily,
            InputMaps = [.. _inputMaps.Select(kvp => new InputMap() { InputId = kvp.Key, ActionName = kvp.Value })]
        };

    #endregion
}
