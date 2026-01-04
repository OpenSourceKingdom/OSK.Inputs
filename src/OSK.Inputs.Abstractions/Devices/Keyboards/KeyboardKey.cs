using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.Keyboards;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardKey(int id) : PhysicalInput(KeyboardInputs.KeyboardDeviceType, id, InputType.Digital), IKeyboardInput
{
}
