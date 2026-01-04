using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.Mice;

namespace OSK.Inputs.Abstractions.Devices.Mice;

public class MouseScrollWheel(int id): PhysicalInput(MouseInputs.MouseDeviceType, id, InputType.Analog)
{
}
