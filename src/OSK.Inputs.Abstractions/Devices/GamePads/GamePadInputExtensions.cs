using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public static class GamePadInputExtensions
{
    internal static IInput ToInput(this GamePadInput gamePadInput)
        => gamePadInput switch
        {
            GamePadInput.LeftTrigger or GamePadInput.RightTrigger
            or GamePadInput.LeftJoyStick or GamePadInput.RightJoyStick => new AnalogInput(InputDeviceType.GamePad, (int)gamePadInput),
            _ => new DigitalInput(InputDeviceType.GamePad, (int)gamePadInput)
        };
}
