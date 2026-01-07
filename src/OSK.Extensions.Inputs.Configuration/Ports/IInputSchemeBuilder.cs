using System;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Extensions.Inputs.Configuration.Ports;

/// <summary>
/// A builder to fluently create input schemes
/// </summary>
public interface IInputSchemeBuilder
{
    /// <summary>
    /// Adds a device map for a given specification and an actionc configurator
    /// </summary>
    /// <typeparam name="TDeviceSpecification">The device to make a map for</typeparam>
    /// <param name="mapBuilderConfigurator">The action configurator</param>
    /// <returns>The builder for chaining</returns>
    IInputSchemeBuilder WithDevice<TDeviceSpecification>(Action<IInputDeviceMapBuilder> mapBuilderConfigurator)
        where TDeviceSpecification : InputDeviceSpecification, new();

    /// <summary>
    /// Makes the scheme a default scheme
    /// </summary>
    /// <returns>The builder for chaining</returns>
    IInputSchemeBuilder MakeDefault();
}
