using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Ports;

/// <summary>
/// A builder that helps to more fluently create configuration for device maps, using a typed specification for input guarding
/// </summary>
/// <typeparam name="TDeviceSpecification">The specification the builder is configuring with</typeparam>
/// <typeparam name="TInput">The input the specification uses</typeparam>
public interface IInputDeviceMapBuilder<TDeviceSpecification, TInput>
    where TInput: IInput
    where TDeviceSpecification: InputDeviceSpecification<TInput>, new()
{
    /// <summary>
    /// Adds an input map
    /// </summary>
    /// <param name="input">The input to add</param>
    /// <param name="actionName">The action the input maps to</param>
    /// <returns>The builder for chaining</returns>
    IInputDeviceMapBuilder<TDeviceSpecification, TInput> WithInputMap(TInput input, string actionName);
}
