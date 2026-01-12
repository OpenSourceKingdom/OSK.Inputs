using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Mice;

public static class MouseInputExtensions
{
    public static IInput ToInput(this MouseInput mouseInput)
        => mouseInput switch
        {
            MouseInput.MouseMovement => new PointerInput(InputDeviceType.Mice, (int)mouseInput),
            _ => new DigitalInput(InputDeviceType.Mice, (int)mouseInput)
        };
}
