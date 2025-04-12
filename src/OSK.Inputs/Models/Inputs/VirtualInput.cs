namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// Represents a software created input and is not necessarily confined to a physical object. i.e. combination inputs, etc. 
/// </summary>
/// <param name="id">The unique id of the input</param>
/// <param name="name">The name of the input</param>
/// <param name="deviceType">The type of device that an input belongs to</param>
public abstract class VirtualInput(int id, string name, string deviceType): IInput
{
    public int Id => id;

    public string Name => name;

    public string DeviceType => deviceType;
}
