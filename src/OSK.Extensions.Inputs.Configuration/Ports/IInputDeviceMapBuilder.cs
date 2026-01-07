namespace OSK.Extensions.Inputs.Configuration.Ports;

/// <summary>
/// A builder that helps to more fluently create configuration for device maps
/// </summary>
public interface IInputDeviceMapBuilder
{
    /// <summary>
    /// Adds an input map
    /// </summary>
    /// <param name="inputId">The id of the input to add that is on the associated devive</param>
    /// <param name="actionName">The action the input maps to</param>
    /// <returns>The builder for chaining</returns>
    IInputDeviceMapBuilder WithInputMap(int inputId, string actionName);
}
