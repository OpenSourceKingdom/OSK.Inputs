using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// The contextual event information for an input that triggered an action and that a system can respond to.
/// </summary>
/// <param name="userId">The user who initiated the input</param>
/// <param name="deltaTime">The amount of time that has occurred since the last frame update</param>
/// <param name="inputEvent">The input that triggered the event</param>
/// <param name="inputStreamDetails">Additional input stream details that are optionally added to an action</param>
/// <param name="activityInformation">The specific activity information for this event</param>
/// <param name="serviceProvider">The services available to this event context</param>
public class InputEventContext(int userId, TimeSpan deltaTime, InputEvent inputEvent, 
    IEnumerable<InputStreamDetails> inputStreamDetails, InputActivityInformation activityInformation, IServiceProvider serviceProvider)
{
    #region Variables

    public bool ConsumedInput { get; private set; } = true;

    private readonly Dictionary<Type, InputStreamDetails> _streamDetails = inputStreamDetails?.ToDictionary(streamDetails => streamDetails.GetType()) ?? [];

    private readonly Dictionary<InputAxis, InputIntensity> _intensityLookup
        = (inputEvent switch
        {
            InputPowerEvent powerEvent => powerEvent.InputIntensities,
            PointerInputStreamEvent pointerEvent => GetPointerIntensities(pointerEvent, inputStreamDetails.OfType<PointerStreamDetails>().First()),
            VirtualInputEvent _ => [InputIntensity.Full(InputAxis.X)],
            _ => []
        }).ToDictionary(intensity => intensity.Axis);

    /// <summary>
    /// The user who initiated the event
    /// </summary>
    public int UserId => userId;

    /// <summary>
    /// The amount of time that has occurred since the last frame was processed
    /// </summary>
    public TimeSpan DeltaTimeSinceLastFrame => deltaTime;

    /// <summary>
    /// The input event that triggered the <see cref="InputAction"/> in the input system
    /// </summary>
    public InputEvent Event => inputEvent;

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

    /// <summary>
    /// Attempts to get the details of a given type from the context
    /// </summary>
    /// <typeparam name="TDetails">The type of <see cref="InputStreamDetails"/></typeparam>
    /// <param name="streamDetails">The details object, if found</param>
    /// <returns>Whether the details were in the context</returns>
    public bool TryGetStreamDetails<TDetails>([NotNullWhen(true)] out TDetails? streamDetails)
        where TDetails : InputStreamDetails
    {
        _streamDetails.TryGetValue(typeof(TDetails), out var details);

        streamDetails = details as TDetails;
        return streamDetails is not null;
    }

    /// <summary>
    /// Signals to the input system that the current input should not be considered 'consumed', that is other processes that could intercept and process the input should
    /// be ran.
    /// </summary>
    public void DoNotConsumeInput()
    {
        ConsumedInput = false;
    }

    /// <summary>
    /// Gets the <see cref="InputIntensity"/> of a given input axis
    /// </summary>
    /// <param name="axis">The axis for the input intensity</param>
    /// <returns>The intensity of that input axis</returns>
    public InputIntensity GetInputIntensity(InputAxis axis)
        => _intensityLookup.TryGetValue(axis, out var intensity)
            ? intensity
            : InputIntensity.Zero(axis);

    private static IEnumerable<InputIntensity> GetPointerIntensities(PointerInputStreamEvent pointerEvent, PointerStreamDetails details)
    {
        return [];
    }

    #endregion
}
