using System;
using Microsoft.Extensions.DependencyInjection;
using OSK.Extensions.Inputs.Configuration.Internal.Services;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds an <see cref="InputSystemConfiguration"/> to the dependency container
        /// </summary>
        /// <param name="inputSystemConfigurator">The configurator for the configuration</param>
        /// <returns>The service collection for chaining</returns>
        /// <exception cref="ArgumentNullException">configurator can not be null</exception>
        public IServiceCollection AddInputSystemConfigurationContainerSource(Action<IInputSystemConfigurationBuilder> inputSystemConfigurator)
        {
            if (inputSystemConfigurator is null)
            {
                throw new ArgumentNullException(nameof(inputSystemConfigurator));
            }

            services.AddTransient<IInputSystemConfigurationSource>(_ =>
            {
                var configurationBuilder = new InputSystemConfigurationBuilder();
                inputSystemConfigurator(configurationBuilder);

                return new InputSystemConfigurationContainerSource(configurationBuilder.Build());
            });

            return services;
        }
    }
}
