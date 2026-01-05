using System;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

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
    /// <typeparam name="TInput">The type of input the device uses</typeparam>
    /// <param name="mapBuilderConfigurator">The action configurator</param>
    /// <returns>The builder for chaining</returns>
    IInputSchemeBuilder WithDevice<TDeviceSpecification, TInput>(Action<IInputDeviceMapBuilder<TDeviceSpecification, TInput>> mapBuilderConfigurator)
        where TInput : Enum
        where TDeviceSpecification : InputDeviceSpecification<TInput>, new();

    /// <summary>
    /// Makes the scheme a default scheme
    /// </summary>
    /// <returns>The builder for chaining</returns>
    IInputSchemeBuilder MakeDefault();
}
