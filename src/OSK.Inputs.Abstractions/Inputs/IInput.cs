using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// An input for an <see cref="InputDeviceSpecification"/>
/// </summary>
public interface IInput
{
    /// <summary>
    /// The type of device the input belongs to
    /// </summary>
    InputDeviceType DeviceType { get; }
}
