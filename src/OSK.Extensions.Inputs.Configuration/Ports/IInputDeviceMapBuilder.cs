using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Ports;

/// <summary>
/// A builder that helps to more fluently create configuration for device maps
/// </summary>
public interface IInputDeviceMapBuilder
{        
    /// <summary>
    /// Adds an input map
    /// </summary>
    /// <param name="inputId">The id of the input to add that is on the associated device</param>
    /// <param name="actionName">The action the input maps to</param>
    /// <returns>The builder for chaining</returns>
    IInputDeviceMapBuilder WithInputMap(int inputId, string actionName);

    /// <summary>
    /// Adds an input stream that provides a flow of continuous data
    /// </summary>
    /// <param name="inputId">The id of the input to add that is on the associated device</param>
    /// <param name="actionName">The action the input stream maps to. For an input stream, this is not required as the stream of data can provide contextual information beyond an action trigger.</param>
    /// <returns>The builder for chaining</returns>
    IInputDeviceMapBuilder WithInputStream<TInputStream>(int inputId, string? actionName = null)
        where TInputStream: InputStream;

    /// <summary>
    /// Create a map using a virtual input consisting of inputs on the device
    /// </summary>
    /// <typeparam name="TVirtualInput">The type of virtual input that will take a device type and list of input ids as parameters</typeparam>
    /// <param name="inputIds">The inputs the virtual map uses</param>
    /// <param name="actionName">The action the input maps to</param>
    /// <returns>The builder for chaining</returns>
    IInputDeviceMapBuilder WithVirtualInput<TVirtualInput>(int[] inputIds, string actionName)
        where TVirtualInput: VirtualInput;
}
