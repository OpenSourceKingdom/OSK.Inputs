using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class GamePadJoyStick(int id): PhysicalInput(GamePadInputs.GamePadDeviceType, id, InputType.Analog), IGamePadInput
{
}
