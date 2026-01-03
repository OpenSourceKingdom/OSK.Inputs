using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OSK.Extensions.Inputs.Configuration.Internal.Services;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions;

namespace OSK.Extensions.Inputs.Configuration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
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


        public void Add()
        {
            services.AddInputSystemConfigurationContainerSource(builder =>
            {
                builder.WithInputDefinition("Abc", definitionBuilder =>
                {
                });
            });
        }
    }
}
