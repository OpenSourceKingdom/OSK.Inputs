namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// This input is more of a continous input. i.e. it can hold values that are not simply 0 or 1 (ON/OFF). Think of this as a joystick or throttle style input where 
/// the input value could range from 0..1
/// </summary>
/// <param name="name">The name of the input</param>
public class AnalogInput(string name): HardwareInput(name)
{
}
