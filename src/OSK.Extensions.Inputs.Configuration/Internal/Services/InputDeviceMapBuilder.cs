using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputDeviceMapBuilder(InputDeviceIdentity deviceIdentity) : IInputDeviceMapBuilder
{
    #region Variables

    private readonly Dictionary<int, string> _inputMaps = [];

    #endregion

    #region IInputDeviceMapBuilder

    public IInputDeviceMapBuilder WithInputMap(int inputId, string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentNullException(nameof(actionName));
        }

        _inputMaps[inputId] = actionName;
        return this;
    }

    #endregion

    #region Helpers

    internal InputDeviceMap Build()
        => new()
        {
            DeviceIdentity = deviceIdentity,
            InputMaps = [.. _inputMaps.Select(kvp => new InputMap() { InputId = kvp.Key, ActionName = kvp.Value })]
        };

    #endregion
}
