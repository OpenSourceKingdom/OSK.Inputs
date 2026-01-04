using System;
using System.Collections.Generic;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Extensions.Inputs.Configuration.Ports;

/// <summary>
/// A builder to help create input system confiugration more fluently
/// </summary>
public interface IInputDefinitionBuilder
{
    /// <summary>
    /// Adds an input action to the definition, using an action configuration
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="executor">The action to execute when triggered</param>
    /// <param name="triggerPhases">The phases the action should be triggered</param>
    /// <param name="actionOptions">The options that can be set for the action</param>
    /// <returns>The builder for chaining</returns>
    IInputDefinitionBuilder WithAction(string name, Action<InputEventContext> executor, IEnumerable<InputPhase> triggerPhases,
        InputActionOptions actionOptions);

    /// <summary>
    /// Adds a given input scheme to the definition, using an action configuration
    /// </summary>
    /// <param name="name">The name of the scheme</param>
    /// <param name="schemeBuildConfiguration">The configurator action</param>
    /// <returns>The builder for chaining</returns>
    IInputDefinitionBuilder WithScheme(string name, Action<IInputSchemeBuilder> schemeBuildConfiguration);

    /// <summary>
    /// Makes the definition the default definition to use
    /// </summary>
    /// <returns>The builder for chaining</returns>
    IInputDefinitionBuilder MakeDefault();
}
