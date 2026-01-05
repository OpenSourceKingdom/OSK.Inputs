using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class GamePadDeviceInput(GamePadInput input): Input(InputDeviceType.GamePad, (int)input), IGamePadInput
{
}
