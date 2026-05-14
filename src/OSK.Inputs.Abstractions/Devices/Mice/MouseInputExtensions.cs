using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Mice;

public static class MouseInputExtensions
{
    internal static DeviceInput ToInput(this MouseInput mouseInput)
        => mouseInput switch
        {
            MouseInput.MouseMovement => new PointerInputStream(InputDeviceType.Mice, (int)mouseInput),
            _ => new DigitalInput(InputDeviceType.Mice, (int)mouseInput)
        };
}
