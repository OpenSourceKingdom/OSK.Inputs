using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputSchemeBuilder(string name) : IInputSchemeBuilder
{
    #region Variables

    private bool _isDefault;

    private readonly Dictionary<InputDeviceIdentity, Action<IInputDeviceMapBuilder>> _mapBuilderConfigurators = [];

    #endregion

    #region IInputSchemeBuilder

    public IInputSchemeBuilder MakeDefault()
    {
        _isDefault = true;
        return this;
    }

    public IInputSchemeBuilder WithDevice(InputDeviceIdentity deviceIdentity, Action<IInputDeviceMapBuilder> mapBuilderConfigurator)
    {
        if (mapBuilderConfigurator is null)
        {
            throw new ArgumentNullException(nameof(mapBuilderConfigurator));
        }

        _mapBuilderConfigurators[deviceIdentity] = mapBuilderConfigurator;
        return this;
    }

    #endregion

    #region Helpers

    internal InputScheme Build()
    {
        var deviceMaps = _mapBuilderConfigurators.Select(deviceMapKvp =>
        {
            var deviceMapBuilder = new InputDeviceMapBuilder(deviceMapKvp.Key);
            deviceMapKvp.Value(deviceMapBuilder);

            return deviceMapBuilder.Build();
        });

        return new InputScheme(name, deviceMaps, _isDefault, false);
    }

    #endregion
}
