using System;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal abstract class InputState
{
    public required RuntimeDeviceIdentifier DeviceIdentifier { get; init; }

    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    public InputPhase Phase { get; set; }

    public InputActionMap? MappedAction {  get; set; }

    public TimeSpan? InactiveDuration { get; set; }
}
