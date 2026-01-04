using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.GamePads;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class GamePadTrigger(int id): PhysicalInput(GamePadInputs.GamePadDeviceType, id, InputType.Analog)
{
}
