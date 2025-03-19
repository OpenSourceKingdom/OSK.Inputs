using System;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;
internal class DefaultInputReaderProvider(IServiceProvider serviceProvider) : IInputReaderProvider
{
    #region IInputReaderProvider

    public IInputReader GetInputReader(IInputControllerConfiguration controllerConfiguration, InputControllerIdentifier controllerIdentifier)
    {
        if (controllerConfiguration is null)
        {
            throw new ArgumentNullException(nameof(controllerConfiguration));
        }

        return (IInputReader)ActivatorUtilities.CreateInstance(serviceProvider, controllerConfiguration.InputReaderType, controllerIdentifier, controllerConfiguration);
    }

    #endregion
}
