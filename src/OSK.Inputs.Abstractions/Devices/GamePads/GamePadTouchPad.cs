using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class GamePadTouchPad(int id): PhysicalInput(GamePadInputs.GamePadDeviceType, id, InputType.Pointer), IGamePadInput
{
}
