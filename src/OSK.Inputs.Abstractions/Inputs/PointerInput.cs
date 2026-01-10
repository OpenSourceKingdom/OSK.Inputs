using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// Represents input that moves a point location on the screen
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">A unique id for the input</param>
public class PointerInput(InputDeviceType deviceType, int id) : Input(deviceType, id), IDeviceInput
{
}
