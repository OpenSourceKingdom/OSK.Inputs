using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardCombination(KeyboardInput input, params KeyboardInput[] keys)
    : DeviceCombinationInput(InputDeviceType.Keyboard, (int)input, [..keys.Select(key => new KeyboardKeyInput(key))]), IKeyboardInput
{
}
