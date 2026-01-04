using System;
using OSK.Extensions.Inputs.Configuration.Internal.Services;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration;

/// <summary>
/// A factory to create <see cref="InputSystemConfiguration"/> without dependency injection
/// </summary>
public static class InputSystemConfigurationFactory
{
    /// <summary>
    /// Creates an input system configuration provided a configurator
    /// </summary>
    /// <param name="configurator">The configurator for the input system configuratoion</param>
    /// <returns>The completed <see cref="InputSystemConfiguration"/></returns>
    public static InputSystemConfiguration Create(Action<IInputSystemConfigurationBuilder> configurator)
    {
        if (configurator is null)
        {
            throw new ArgumentNullException(nameof(configurator));
        }

        var builder = new InputSystemConfigurationBuilder();
        configurator(builder);

        return builder.Build();
    }
}
