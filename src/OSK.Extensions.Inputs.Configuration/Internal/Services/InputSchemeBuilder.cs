using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputSchemeBuilder(string name, IInputSystemConfigurationBuilder configurationBuilder) : IInputSchemeBuilder
{
    #region Variables

    private bool _isDefault;

    private readonly Dictionary<InputDeviceFamily, DeviceInputMap> _maps = [];

    #endregion

    #region IInputSchemeBuilder

    public IInputSchemeBuilder MakeDefault()
    {
        _isDefault = true;
        return this;
    }

    public IInputSchemeBuilder WithDevice<TDeviceSpecification, TInput>(Action<IInputDeviceMapBuilder<TDeviceSpecification, TInput>> mapBuilderConfigurator)
        where TInput : Enum
        where TDeviceSpecification : InputDeviceSpecification<TInput>, new()
    {
        if (mapBuilderConfigurator is null)
        {
            throw new ArgumentNullException(nameof(mapBuilderConfigurator));
        }

        var deviceSpecification = new TDeviceSpecification();
        configurationBuilder.WithDevice(deviceSpecification);

        var mapBuilder = new InputDeviceMapBuilder<TDeviceSpecification, TInput>(deviceSpecification.DeviceFamily);
        mapBuilderConfigurator(mapBuilder);

        var map = mapBuilder.Build(); ;

        var deviceInputLookup = deviceSpecification.GetInputs().ToDictionary(input => input.Id);
        var invalidInputs = map.InputMaps.Where(input => !deviceInputLookup.TryGetValue(input.InputId, out _));
        if (invalidInputs.Any())
        {
            throw new InvalidOperationException($"The input scheme {name} was configured with one or more inputs that were not valid for the device {deviceSpecification.DeviceFamily}, these inputs were: {string.Join(", ", invalidInputs.Select(map => $"{map.InputId} - {map.ActionName}"))}.");
        }

        _maps[deviceSpecification.DeviceFamily] = map;
        return this;
    }

    #endregion

    #region Helpers

    internal InputScheme Build()
    {
        return new InputScheme(name, _maps.Values, _isDefault, false);
    }

    #endregion
}
