using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Represents a unique device attached to the game system. It consists of the device identity and the actual id that was assigned 
/// to it
/// </summary>
/// <param name="DeviceId">The game system id for the device</param>
/// <param name="DeviceFamily">The specific <see cref="InputDeviceFamily"/> associated with the device</param>
public readonly record struct RuntimeDeviceIdentifier(int DeviceId, InputDeviceFamily DeviceFamily)
{
}
