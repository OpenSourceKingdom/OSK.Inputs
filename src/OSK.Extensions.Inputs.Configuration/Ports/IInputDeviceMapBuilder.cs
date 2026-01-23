using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Ports;

/// <summary>
/// A builder that helps to more fluently create configuration for device maps
/// </summary>
public interface IInputDeviceMapBuilder
{
    /// <summary>
    /// Adds an input that is passive in nature
    /// </summary>
    /// <param name="inputId">The id of the input to add that is on the associated device</param>
    /// <returns>The builder for chaining</returns>
    IInputDeviceMapBuilder WithPassiveInput(int inputId);
        
    /// <summary>
    /// Adds an input map
    /// </summary>
    /// <param name="inputId">The id of the input to add that is on the associated device</param>
    /// <param name="actionName">The action the input maps to</param>
    /// <returns>The builder for chaining</returns>
    IInputDeviceMapBuilder WithInputMap(int inputId, string actionName);

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
