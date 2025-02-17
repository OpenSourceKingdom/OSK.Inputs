using OSK.Inputs.Ports;

namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// A <see cref="HardwareInput"/> that also has a specific value and option type
/// </summary>
/// <typeparam name="TOptions">The type of options for the input</typeparam>
/// <typeparam name="TValue">The type of value for the input</typeparam>
/// <param name="name">The name of the input</param>
/// <param name="value">The value this input represents</param>
/// <param name="options">A set of options that will be used to help process the input on an <see cref="IInputReceiver"/></param>
public abstract class HardwareInput<TValue, TOptions>(string name, TValue value, TOptions options)
    : HardwareInput(name)
{
    public TOptions Options => options;

    public TValue Value => value;
}
