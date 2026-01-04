using System;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Ports;

/// <summary>
/// A builder that helps to create <see cref="InputSystemConfiguration"/> in a more fluent manner
/// </summary>
public interface IInputSystemConfigurationBuilder
{
    /// <summary>
    /// Adds a join policy using an action configurator
    /// </summary>
    /// <param name="optionsConfigurator">The action configurator</param>
    /// <returns>The builder for chaining</returns>
    IInputSystemConfigurationBuilder WithJoinPolicy(Action<InputSystemJoinPolicyOptions> optionsConfigurator);

    /// <summary>
    /// Adds input processing options to the configuration using an action configurator
    /// </summary>
    /// <param name="optionsConfigurator">The action configurator</param>
    /// <returns>The builder for chaining</returns>
    IInputSystemConfigurationBuilder WithInputProcessing(Action<InputProcessingOptions> optionsConfigurator);

    /// <summary>
    /// Adds a definition to the configuration using an action configurator
    /// </summary>
    /// <param name="definitionName">The definition name</param>
    /// <param name="definitionBuildConfigurator">The action configurator</param>
    /// <returns>The builder for chaining</returns>
    IInputSystemConfigurationBuilder WithInputDefinition(string definitionName, Action<IInputDefinitionBuilder> definitionBuildConfigurator);

    /// <summary>
    /// Adds a device specification to the configuration
    /// </summary>
    /// <param name="specification">The device specification the input system will support</param>
    /// <returns>The builder for chaining</returns>
    IInputSystemConfigurationBuilder WithDevice(InputDeviceSpecification specification);

    /// <summary>
    /// Builds a configuration given the current applied settings
    /// </summary>
    /// <returns>The configuration</returns>
    InputSystemConfiguration Build();
}
