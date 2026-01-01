using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

public class DeviceInputActionMap
{
    public int InputId => Input.Id;

    public required Input Input { get; init; }

    public required InputAction Action { get; init; }

    public required int[] LinkedInputIds { get; init; }
}
