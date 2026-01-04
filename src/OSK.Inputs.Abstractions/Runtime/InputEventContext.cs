using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// The contextual event information for an input that triggered an action and that a system can respond to.
/// </summary>
/// <param name="userId">The user who initiated the input</param>
/// <param name="inputEvent">The input that triggered the event</param>
/// <param name="pointerDetails">Pointer information, if available</param>
/// <param name="activityInformation">The specific activity information for this event</param>
/// <param name="serviceProvider">The services available to this event context</param>
public class InputEventContext(int userId, InputEvent inputEvent, PointerDetails pointerDetails, 
    InputActivityInformation activityInformation, IServiceProvider serviceProvider)
{
    #region Variables

    private readonly Dictionary<InputAxis, InputIntensity> _intensityLookup
        = (inputEvent switch
        {
            InputPowerEvent powerEvent => powerEvent.InputIntensities,
            InputPointerEvent pointerEvent => GetPointerIntensities(pointerEvent, pointerDetails),
            VirtualInputEvent _ => [InputIntensity.Full(InputAxis.X)],
            _ => []
        }).ToDictionary(intensity => intensity.Axis);

    /// <summary>
    /// The user who initiated the event
    /// </summary>
    public int UserId => userId;

    /// <summary>
    /// The input event that triggered the <see cref="InputAction"/> in the input system
    /// </summary>
    public InputEvent Event => inputEvent;

    /// <summary>
    /// Provides specific information related to any points associated to the user
    /// </summary>
    public PointerDetails PointerDetails => pointerDetails;

    /// <summary>
    /// Additional information related to the input, potentially across multiple input events
    /// </summary>
    public InputActivityInformation ActivityInformation => activityInformation;

    /// <summary>
    /// The services associated with this event context
    /// </summary>
    public IServiceProvider Services => serviceProvider;

    #endregion

    #region Helpers

    public InputIntensity GetInputIntensity(InputAxis axis)
        => _intensityLookup.TryGetValue(axis, out var intensity)
            ? intensity
            : InputIntensity.Zero(axis);

    private static IEnumerable<InputIntensity> GetPointerIntensities(InputPointerEvent pointerEvent, PointerDetails details)
    {
        return [];
    }

    #endregion
}
