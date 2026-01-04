using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardCombination(int id, params KeyboardKey[] keys): CombinationInput(KeyboardInputs.KeyboardDeviceType, id, keys), IKeyboardInput
{
}
