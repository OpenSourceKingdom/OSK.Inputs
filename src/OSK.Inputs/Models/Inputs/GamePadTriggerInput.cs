using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Inputs;

public class GamePadTriggerInput(int id, string name) : AnalogInput(id, name, GamePadDevice.DeviceTypeName), IGamePadInput
{
}
