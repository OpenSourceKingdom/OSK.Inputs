using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInputs(this IServiceCollection services, Action<IInputBuilder> builderConfiguration)
    {
        if (builderConfiguration is null)
        {
            throw new ArgumentNullException(nameof(builderConfiguration));
        }

        services.TryAddTransient<IInputDefinitionBuilder, InputDefinitionBuilder>();
        services.TryAddTransient<IInputManager, InputManager>();
        services.TryAddTransient<IInputHandler, InputHandler>();
        services.TryAddTransient<IInputValidationService, InputValidationService>();

        var builder = new InputBuilder(services);
        builderConfiguration(builder);
        builder.ApplyInputDefinitions();

        return services;
    }
}
