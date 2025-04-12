namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// This input is more of a continous input. i.e. it can hold values that are not simply 0 or 1 (ON/OFF). Think of this as a joystick or throttle style input where 
/// the input value could range from 0..1
/// </summary>
/// <param name="id">The unique id of the input</param>
/// <param name="name">The name of the input</param>
/// <param name="deviceType">The type of device that an input belongs to</param>
public class AnalogInput(int id, string name, string deviceType): HardwareInput(id, name, deviceType)
{
}
