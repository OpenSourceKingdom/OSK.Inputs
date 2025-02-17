using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Inputs;

public abstract class DigitalInput<TValue>(string name, TValue value, DigitalInputOptions options) 
    : HardwareInput<TValue, DigitalInputOptions>(name, value, options)
{
}
