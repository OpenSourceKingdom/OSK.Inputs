using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardKeyInput(KeyboardInput input) : DigitalInput(InputDeviceType.Keyboard, (int)input)
{
}