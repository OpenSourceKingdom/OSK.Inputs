using System;
using System.Collections.Generic;
using System.Text;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;

namespace OSK.Inputs.Internal;

internal interface IInputUserTracker
{
    ActiveInputScheme ActiveScheme { get; }

    int UserId { get; }

    IEnumerable<TriggeredActionEvent> Update(TimeSpan deltaTime);

    IOutput<TriggeredActionEvent?> Track(InputEvent inputActivation);
}
