using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Inputs;
public class GamePadButtonInput(int id, string name): HardwareInput(id, name, GamePadDevice.DeviceTypeName)
{
}
