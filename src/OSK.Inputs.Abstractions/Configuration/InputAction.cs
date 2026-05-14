using System;
using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Represents an action that can be executed in the input system. This is configured and then retrieved to execute functions
/// in an application that integrates with the input system.
/// </summary>
/// <param name="actionName">The unique name for the action</param>
/// <param name="triggerPhases">The phases considered valid to trigger the action</param>
/// <param name="actionExecutor">The specific action to execute</param>
/// <param name="inputStreams">A list of desired input streams to include in the event context</param>
/// <param name="description">A readable description for the action that can be displayed for users</param>
/// <param name="actionGroup">An option group number that specifies the action group this action belongs to</param>
public class InputAction(string actionName, ISet<InputPhase> triggerPhases, Action<InputEventContext> actionExecutor,
    IEnumerable<InputStreamType>? inputStreams = null, string? description = null, int? actionGroup = null)
{
    #region Api

    /// <summary>
    /// A unique action name
    /// </summary>
    public string Name => actionName;

    /// <summary>
    /// Helper text to be displayed with the action on a device scheme settings screen or similar
    /// </summary>
    public string? Description => description;

    /// <summary>
    /// A collection of additional data stream information that should be calculated and included on an event context when the action is executed.
    /// 
    /// <br />
    /// Note: For example, calculating pointer or similar data stream information may require checking the collection of pointers or associated data to a given user and 
    /// determining any <see cref="PointerMotion"/> information associated with them over recent frames. As such, this could
    /// be a slight performance cost to turn on though the information is only captured for a short time. This should be used with actions that actually need the pointer 
    /// information for their actions.
    /// </summary>
    public IEnumerable<InputStreamType> InputStreams { get; } = inputStreams ?? [];

    /// <summary>
    /// The specific input phases that will trigger this action
    /// </summary>
    public ISet<InputPhase> TriggerPhases => triggerPhases;

    /// <summary>
    /// Specifies an action group for the input action. This can be used in conjunction with <see cref="InputEventProcessOptions.SuppressedActionGroups"/> to ignore
    /// actions of a given type during input processing
    /// </summary>
    public int? ActionGroup => actionGroup;

    /// <summary>
    /// The configured action to execute when the related input is activated
    /// </summary>
    public Action<InputEventContext> ActionExecutor => actionExecutor;

    #endregion
}
