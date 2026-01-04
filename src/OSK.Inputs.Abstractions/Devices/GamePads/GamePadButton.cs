using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class GamePadButton(int id): PhysicalInput(GamePadInputs.GamePadDeviceType, id, InputType.Digital), IGamePadInput
{
}
