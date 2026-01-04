using System;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal abstract class InputState(int inputId)
{
    public required RuntimeDeviceIdentifier DeviceIdentifier { get; init; }

    public required TimeSpan Duration { get; set; }

    public int InputId => inputId;

    public InputPhase Phase { get; set; }

    public InputActionMap? MappedAction {  get; set; }

    public TimeSpan? InactiveDuration { get; set; }
}
