using System;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class InputActionAttribute : Attribute
{
    #region Variables

    internal int? InternalActionGroup { get; private set; } 
    private string? _actionName;

    /// <summary>
    /// A custom action name to use instead of the method name
    /// </summary>
    public string? ActionName 
    { 
        get => _actionName; 
        set
        {
            _actionName = value?.Trim();
        } 
    }

    /// <summary>
    /// A user friendly description of this action, for UI display
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The desired trigger phases for the action
    /// </summary>
    public InputPhase[] TriggerPhases { get; set; } = [];

    /// <summary>
    /// This represents the streams of data that should be included on the event context for a triggered action
    /// </summary>
    public InputStreamType[] IncludedInputStreams { get; set; } = [];

    /// <summary>
    /// Specifies an action type for the input action. This can be used in conjunction with <see cref="InputEventProcessOptions.SuppressedActionGroups"/> to ignore
    /// actions of a given type during input processing
    /// </summary>
    public int ActionGroup 
    {
        get => InternalActionGroup.GetValueOrDefault();
        set => InternalActionGroup = value;
    }

    #endregion
}
