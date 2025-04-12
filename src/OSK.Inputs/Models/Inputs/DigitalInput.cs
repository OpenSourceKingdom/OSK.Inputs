namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// Represents an input that can only represent ON/OFF (essentially, values 0 or 1)
/// </summary>
/// <param name="id">The unique id of the input</param>
/// <param name="name"></param>
/// <param name="deviceType">The type of device that an input belongs to</param>
public class DigitalInput(int id, string name, string deviceType) 
    : HardwareInput(id, name, deviceType)
{
}
