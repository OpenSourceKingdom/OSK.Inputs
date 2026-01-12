using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Devices.Mice;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public static class GamePadInputExtensions
{
    public static IInput ToInput(this GamePadInput gamePadInput)
        => gamePadInput switch
        {
            GamePadInput.LeftTrigger or GamePadInput.RightTrigger
            or GamePadInput.LeftJoyStick or GamePadInput.RightJoyStick => new AnalogInput(InputDeviceType.GamePad, (int)gamePadInput),
            _ => new DigitalInput(InputDeviceType.GamePad, (int)gamePadInput)
        };
}
