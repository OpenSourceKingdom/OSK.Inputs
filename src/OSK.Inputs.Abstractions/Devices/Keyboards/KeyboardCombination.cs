using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.Keyboards;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardCombination(int id, params KeyboardKey[] keys): CombinationInput(KeyboardInputs.KeyboardDeviceType, id, keys), IKeyboardInput
{
}
