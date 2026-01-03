using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Internal.Services;

internal class InputSystemConfigurationContainerSource(InputSystemConfiguration configuration) : IInputSystemConfigurationSource
{
    #region IInputSystemConfigurationSource

    public InputSystemConfiguration GetConfiguration()
    {
        return configuration;
    }

    #endregion
}
