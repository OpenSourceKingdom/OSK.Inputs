using System;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;
internal class DefaultInputReaderProvider(IServiceProvider serviceProvider) : IInputReaderProvider
{
    #region IInputReaderProvider

    public IInputDeviceReader GetInputReader(IInputDeviceConfiguration deviceConfiguration, InputDeviceIdentifier deviceIdentifier)
    {
        if (deviceConfiguration is null)
        {
            throw new ArgumentNullException(nameof(deviceConfiguration));
        }

        return (IInputDeviceReader)ActivatorUtilities.CreateInstance(serviceProvider, deviceConfiguration.InputReaderType, 
            new InputReaderParameters(deviceIdentifier, deviceConfiguration.Inputs));
    }

    #endregion
}
