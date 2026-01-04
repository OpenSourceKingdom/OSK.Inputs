using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.GamePads;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class GamePadTouchPad(int id): PhysicalInput(GamePadInputs.GamePadDeviceType, id, InputType.Pointer), IGamePadInput
{
}
