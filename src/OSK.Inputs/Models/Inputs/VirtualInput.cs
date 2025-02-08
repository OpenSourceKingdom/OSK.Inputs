namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// Represents a software created input and is not necessarily confined to a physical object. i.e. combination inputs, etc. 
/// </summary>
/// <param name="name">The name of the input</param>
public abstract class VirtualInput(string name): IInput
{
    public string Name => name;
}
