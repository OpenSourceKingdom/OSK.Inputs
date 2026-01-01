using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;

namespace OSK.Inputs.Internal;

internal interface IUserInputTracker
{
    ActiveInputScheme ActiveScheme { get; }

    int UserId { get; }

    IEnumerable<TriggeredActionEvent> Update(TimeSpan deltaTime);

    TriggeredActionEvent? Track(InputEvent inputActivation);
}
