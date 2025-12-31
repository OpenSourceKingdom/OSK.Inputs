using System;
using OSK.Inputs.Abstractions;

namespace OSK.Inputs.Internal.Models;

internal abstract class InputState
{
    public required Input Input { get; init; }

    public required RuntimeDeviceIdentifier DeviceIdentifier { get; init; }

    public required TimeSpan Duration { get; set; }

    public int InputId => Input.Id;

    public InputPhase Phase { get; set; }

    public TimeSpan? InactiveDuration { get; set; }
}
