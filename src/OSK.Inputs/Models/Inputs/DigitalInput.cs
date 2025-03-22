namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// Represents an input that can only represent ON/OFF (essentially, values 0 or 1)
/// </summary>
/// <param name="name"></param>
public class DigitalInput(string name) 
    : HardwareInput(name)
{
}
