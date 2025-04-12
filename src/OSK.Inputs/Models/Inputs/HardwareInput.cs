namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// A hardware input can be physically represented by some real-world object. i.e. Playstation/Xbox controller buttons, Keyboard keys, Mouse wheel, etc.
/// </summary>
/// <param name="id">The unique id of the input</param>
/// <param name="name">The name of the input</param>
/// <param name="deviceType">The type of device that an input belongs to</param>
public abstract class HardwareInput(int id, string name, string deviceType) : IInput
{
    public int Id => id;

    public string DeviceType => deviceType;

    public string Name => name;
}
