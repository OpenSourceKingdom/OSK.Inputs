using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputSchemeBuilder(string inputDefinitionName, IEnumerable<IInputDeviceConfiguration> supportedDevices,
    string schemeName) 
    : IInputSchemeBuilder
{
    #region Variables

    private readonly Dictionary<InputDeviceName, InputDeviceActionMap> _deviceActionMaps = [];
    private bool _isDefault;

    #endregion

    #region IInputSchemeBuilder

    public IInputSchemeBuilder AddDevice<TInput>(InputDeviceName deviceName, Action<IInputDeviceActionBuilder<TInput>> builder)
        where TInput: IInput
    {
        var deviceConfiguration = supportedDevices.FirstOrDefault(device => device.DeviceName == deviceName);
        if (deviceConfiguration is null)
        {
            throw new InvalidOperationException($"Input scheme {schemeName} for input definition {inputDefinitionName} is invalid because the input device {deviceName} is not a supported device.");
        }

        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (_deviceActionMaps.TryGetValue(deviceName, out _))
        {
            throw new DuplicateNameException($"A device action map for device {deviceConfiguration.DeviceName} has already been added on input scheme {schemeName} on input definition {inputDefinitionName}.");
        }

        var deviceActionMapBuilder = new InputDeviceActionBuilder<TInput>(inputDefinitionName, schemeName, deviceConfiguration);
        builder(deviceActionMapBuilder);

        _deviceActionMaps[deviceConfiguration.DeviceName] = deviceActionMapBuilder.Build();
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
        return new BuiltInInputScheme(inputDefinitionName, schemeName, _isDefault, _deviceActionMaps.Values);
    }

    #endregion
}
