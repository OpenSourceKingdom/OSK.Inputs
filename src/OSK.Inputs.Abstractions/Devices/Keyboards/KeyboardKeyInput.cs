using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardKeyInput(KeyboardInput input) : Input(InputDeviceType.Keyboard, (int)input), IKeyboardInput
{
}