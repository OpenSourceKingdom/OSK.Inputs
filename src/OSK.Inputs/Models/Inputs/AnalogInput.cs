using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// This input is more of a continous input. i.e. it can hold values that are not simply 0 or 1 (ON/OFF). Think of this as a joystick or throttle style input where 
/// the input value could range from 0..1
/// </summary>
/// <typeparam name="TValue">The type of value this power input represents</typeparam>
/// <param name="name">The name of the input</param>
/// <param name="value">The value of the input</param>
/// <param name="options">Power input options</param>
public abstract class AnalogInput<TValue>(string name, TValue value, AnalogInputOptions options): HardwareInput<AnalogInputOptions, TValue>(name, value, options)
{
}
