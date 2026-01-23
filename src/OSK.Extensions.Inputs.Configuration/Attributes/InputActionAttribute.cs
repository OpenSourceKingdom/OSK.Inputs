using System;
using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class InputActionAttribute : Attribute
{
    #region Variables

    public string? ActionName { get; }

    public string? Description { get; }

    public IEnumerable<InputPhase> TriggerPhases { get; }

    public bool IncludePointerDetails { get; }

    #endregion

    #region Constructors

    public InputActionAttribute(InputPhase[] triggerPhases, string? actionName = null, bool includePointerDetails = false, string? description = null)
    {
        TriggerPhases = triggerPhases;
        ActionName = actionName?.Trim();
        IncludePointerDetails = includePointerDetails;
        Description = description;
    }

    #endregion
}
