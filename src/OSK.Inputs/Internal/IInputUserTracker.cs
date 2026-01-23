using System;
using System.Collections.Generic;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;

namespace OSK.Inputs.Internal;

internal interface IInputUserTracker
{
    ActiveInputScheme ActiveScheme { get; }

    int UserId { get; }

    void ResetInput(InputDeviceFamily deviceFamily);

    IEnumerable<ProcessedInputEvent> Update(TimeSpan deltaTime);

    IOutput<ProcessedInputEvent> Track(TimeSpan deltaTime, InputEvent inputActivation);
}
