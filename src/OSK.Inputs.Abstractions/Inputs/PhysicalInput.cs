namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// Represents a physical entity that a user can interact with on a device.
/// For example, a key on a keyboard, a button on a controller, etc.
/// </summary>
/// <param name="deviceType">The type of device this input is associated to</param>
/// <param name="id">The device provided id for the input</param>
/// <param name="inputType">The type of input this represents</param>
public abstract class PhysicalInput(string deviceType, int id, InputType inputType)
    : Input(deviceType, id, inputType)
{ 
}
