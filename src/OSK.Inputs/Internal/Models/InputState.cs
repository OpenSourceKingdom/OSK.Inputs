using System;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal abstract class InputState
{
    public required Input Input { get; init; }

    public required RuntimeDeviceIdentifier DeviceIdentifier { get; init; }

    public required TimeSpan Duration { get; set; }

    public int InputId => Input.Id;

    public InputPhase Phase { get; set; }

    public DeviceInputActionMap? MappedAction {  get; set; }

    public TimeSpan? InactiveDuration { get; set; }
}
