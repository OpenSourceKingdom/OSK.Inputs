using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.Mice;

namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Represents a button on a mouse
/// </summary>
/// <param name="id">The unique id for the mouse button</param>
public class MouseButton(int id): PhysicalInput(MouseInputs.MouseDeviceType, id, InputType.Digital)
{
}
