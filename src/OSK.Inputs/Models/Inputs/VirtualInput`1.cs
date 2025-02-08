namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// A <see cref="VirtualInput"/> that also has an option type
/// </summary>
/// <typeparam name="TOptions">The type of options for the button</typeparam>
/// <param name="name">The name of the button</param>
/// <param name="options">The options for the button to use with an input receiver</param>
public abstract class VirtualInput<TOptions>(string name, TOptions options): VirtualInput(name)
{
    public TOptions Options => options;
}
