using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Represents a button on a mouse
/// </summary>
/// <param name="input">The unique id for the mouse button</param>
public class MouseDeviceInput(MouseInput input): Input(InputDeviceType.Mice, (int)input)
{
}
