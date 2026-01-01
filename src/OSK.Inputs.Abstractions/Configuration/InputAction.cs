using System;
using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputAction(string actionName, ISet<InputPhase> triggerPhases, bool trackPointer, Action<InputEventContext> actionExecutor,
    string? description = null)
{
    #region  Variables

    public string Name => actionName;

    public string? Description => description;

    #endregion

    #region Api

    public bool TrackPointer => trackPointer;

    public ISet<InputPhase> TriggerPhases => triggerPhases;

    public void Execute(InputEventContext context)
        => actionExecutor(context);

    #endregion
}
