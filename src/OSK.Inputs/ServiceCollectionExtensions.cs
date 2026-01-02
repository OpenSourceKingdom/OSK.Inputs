using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Functions.Outputs.Logging;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Exceptions;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInputs()
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
    }
}
