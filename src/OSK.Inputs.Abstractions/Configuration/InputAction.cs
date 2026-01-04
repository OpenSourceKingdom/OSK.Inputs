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
/// <param name="includePointerDetails">Determines if pointer data should be calculated and applied to the event context when the action is executed</param>
/// <param name="description">A readable description for the action that can be displayed for users</param>
public class InputAction(string actionName, ISet<InputPhase> triggerPhases, Action<InputEventContext> actionExecutor,
    bool includePointerDetails = false, string? description = null)
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
    /// Whether pointer information should be calculated and included on an event context when the action is executed.
    /// 
    /// <br />
    /// Note: Calculating pointer information requires checking the collection of pointers associated to a given user and 
    /// determining any <see cref="PointerMotion"/> information associated with them over recent frames. As such, this could
    /// be a slight performance cost to turn on though motion information is only captured for a short time. This should be
    /// used with actions that actually need the pointer information for their actions.
    /// </summary>
    public bool IncludePointerDetails => includePointerDetails;

    /// <summary>
    /// The specific input phases that will trigger this action
    /// </summary>
    public ISet<InputPhase> TriggerPhases => triggerPhases;

    /// <summary>
    /// The configured action to execute when the related input is activated
    /// </summary>
    public Action<InputEventContext> ActionExecutor => actionExecutor;

    #endregion
}
