using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardCombination(params KeyboardInput[] keys)
    : DeviceCombinationInput(InputDeviceType.Keyboard, [..keys.Select(key => new KeyboardKeyInput(key))])
{
}
