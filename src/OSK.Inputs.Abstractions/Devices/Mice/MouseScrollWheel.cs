using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Mice;

public class MouseScrollWheel(int id): PhysicalInput(MouseInputs.MouseDeviceType, id, InputType.Analog), IMouseInput
{
}
