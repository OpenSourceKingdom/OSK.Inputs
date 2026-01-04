using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// An input that represents the movement of the mouse
/// </summary>
/// <param name="id">The unique id for the input</param>
public class MouseMovement(int id): PhysicalInput(MouseInputs.MouseDeviceType, id, InputType.Pointer), IMouseInput
{
}
