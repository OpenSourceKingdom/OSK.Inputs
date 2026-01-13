using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Mice;

public static class MouseInputExtensions
{
    internal static IInput ToInput(this MouseInput mouseInput)
        => mouseInput switch
        {
            MouseInput.MouseMovement => new PointerInput(InputDeviceType.Mice, (int)mouseInput),
            _ => new DigitalInput(InputDeviceType.Mice, (int)mouseInput)
        };
}
