using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Functions.Outputs.Logging;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
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

            services.TryAddScoped<IInputProcessor, InputProcessor>();
            services.TryAddScoped<IInputUserManager, InputUserManager>();
            services.TryAddScoped<IInputSystem, InputSystem>();
            services.TryAddScoped<IInputNotificationPublisher, InputNotificationPublisher>();
            services.TryAddScoped<IInputSystemNotifier>(provider
                => provider.GetRequiredService<IInputNotificationPublisher>());

            services.TryAddSingleton<IInputConfigurationProvider>(serviceProvider =>
            {
                var configurationBuilder = serviceProvider.GetRequiredService<IInputSystemConfigurationBuilder>();
                var schemeRepository = serviceProvider.GetService<IInputSchemeRepository>() 
                    ?? new InMemorySchemeRepository(serviceProvider.GetRequiredService<IOutputFactory<InMemorySchemeRepository>>());

                return ActivatorUtilities.CreateInstance<InputConfigurationProvider>(serviceProvider, configurationBuilder.Build());
            });

            return services; 
        }
    }
}
