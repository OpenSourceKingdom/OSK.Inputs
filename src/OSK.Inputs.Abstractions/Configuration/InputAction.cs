using System;
using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputAction(string actionName, ISet<InputPhase> triggerPhases, Action<InputEventContext> actionExecutor,
    bool includePointerDetails = false, string? description = null)
{
    #region  Variables

    public string Name => actionName;

    public string? Description => description;

    #endregion

    #region Api

    public bool IncludePointerDetails => includePointerDetails;

    public ISet<InputPhase> TriggerPhases => triggerPhases;

    public Action<InputEventContext> ActionExecutor => actionExecutor;

    #endregion
}
