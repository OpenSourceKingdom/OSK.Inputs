using System;
using System.Collections.Generic;

namespace OSK.Inputs.Abstractions;

public class InputAction(string actionName, ISet<InputPhase> triggerPhases, bool trackPointer, Action<InputActivationContext> actionExecutor,
    string? description = null)
{
    #region  Variables

    public string Name => actionName;

    public string? Description => description;

    #endregion

    #region Api

    public bool TrackPointer => trackPointer;

    public ISet<InputPhase> TriggerPhases => triggerPhases;

    public void Execute(InputActivationContext context)
        => actionExecutor(context);

    #endregion
}
