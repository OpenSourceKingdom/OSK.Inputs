using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// An input that provides values between -1 to 1 or 0 to 1; a range of values
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">A unique id for the input</param>
public class AnalogInput(InputDeviceType deviceType, int id): DeviceInput(deviceType, id)
{
}
