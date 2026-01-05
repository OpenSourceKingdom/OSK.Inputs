namespace OSK.Inputs.Abstractions.Devices;

/// <summary>
/// Represents a unique input device that a game system can use
/// </summary>
/// <param name="Name">The family of the device</param>
/// <param name="DeviceType">
/// The type of the device. For some devices, the family and type might be identical.
/// For example, a keyboard is a keyboard, but a PlayStation or Xbox Controller is a GamePad
/// </param>
public readonly record struct InputDeviceFamily(string Name, InputDeviceType DeviceType)
{
}
