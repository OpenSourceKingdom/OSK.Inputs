using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardKey(int id) : PhysicalInput(KeyboardInputs.KeyboardDeviceType, id, InputType.Digital), IKeyboardInput
{
}
