using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions;

public class InputActionMap
{
    public int InputId => Input.Id;

    public required Input Input { get; init; }

    public required InputMap Map { get; init; }

    public required InputAction Action { get; init; }
}
