using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Functions.Outputs.Logging;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Exceptions;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core services for the input system and processing to the service collection
    /// </summary>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="InputSystemValidationException">Thrown if the input system configuration provided by the source was invalid</exception>
    public static IServiceCollection AddInputs(this IServiceCollection services)
    {
        services.AddLoggingFunctionOutputs();

        services.TryAddTransient<IInputSystemConfigurationValidator, InputSystemConfigurationValidator>();

        services.TryAddScoped<IInputProcessor, InputProcessor>();
        services.TryAddScoped<IInputUserManager, InputUserManager>();
        services.TryAddScoped<IInputSystem, InputSystem>();
        services.TryAddScoped<IInputNotificationPublisher, InputNotificationPublisher>();
        services.TryAddScoped<IInputSystemNotifier>(provider
            => provider.GetRequiredService<IInputNotificationPublisher>());

        services.TryAddSingleton<IInputConfigurationProvider>(serviceProvider =>
        {
            var configurationSource = serviceProvider.GetRequiredService<IInputSystemConfigurationSource>();
            var validator = serviceProvider.GetRequiredService<IInputSystemConfigurationValidator>();

            var configuration = configurationSource.GetConfiguration();
            var validation = validator.Validate(configuration);
            if (!validation.IsValid)
            {
                throw new InputSystemValidationException($"The input system configuration had a validation error. Configuration Type: {validation.ConfigurationType} Target: {validation.TargetName} Message: {validation.Message}");
            }

            return new InputConfigurationProvider(configuration);
        });

        return services;
    }

    /// <summary>
    /// Adds an <see cref="IInputSchemeRepository"/> that uses an in memory backend, so scheme preferences will not be kept in persistence storage.
    /// This scheme repository does not support custom schemes.
    /// </summary>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInMemorySchemeRepository(this IServiceCollection services)
    {
        services.AddSingleton<IInputSchemeRepository, InMemorySchemeRepository>();

        return services;
    }
}
