using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInputs(this IServiceCollection services, Action<IInputSystemBuilder> builderConfiguration)
    {
        if (builderConfiguration is null)
        {
            throw new ArgumentNullException(nameof(builderConfiguration));
        }

        services.TryAddTransient<IInputDefinitionBuilder, InputDefinitionBuilder>();
        services.TryAddTransient<IInputManager, InputManager>();
        services.TryAddTransient<IInputReaderProvider, DefaultInputReaderProvider>();
        services.TryAddTransient<IInputValidationService, InputValidationService>();

        var builder = new InputSystemBuilder(services);
        builderConfiguration(builder);
        builder.ApplyInputSystemConfiguration();

        return services;
    }
}
