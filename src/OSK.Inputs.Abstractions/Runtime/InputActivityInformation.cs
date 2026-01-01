using System;

namespace OSK.Inputs.Abstractions.Runtime;

public class InputActivityInformation
{
    public int? TapCount { get; set; }

    public TimeSpan Duration { get; set; }
}
