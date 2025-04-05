namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// A hardware input can be physically represented by some real-world object. i.e. Playstation/Xbox controller buttons, Keyboard keys, Mouse wheel, etc.
/// </summary>
/// <param name="name">The name of the input</param>
public abstract class HardwareInput(string name): IInput
{
    public string Name => name;
}
