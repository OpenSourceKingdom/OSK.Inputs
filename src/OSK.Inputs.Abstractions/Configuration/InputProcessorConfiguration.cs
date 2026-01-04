using System;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Specific option configuration for the input processor to use when interpreting <see cref="InputEvent"/>s
/// </summary>
public class InputProcessorConfiguration
{
    /// <summary>
    /// The amount of time the input phase may remain inactive before the interaction
    /// is considered fully ended. If the input is reactivated within this time window,
    /// it is treated as a continuation of the same interaction rather than a new one.
    /// </summary>
    public TimeSpan? TapReactivationTime { get; init; }

    /// <summary>
    /// The minimum amount of time an input must remain in the <see cref="InputPhase.Start"/>
    /// phase before transitioning to <see cref="InputPhase.Active"/>.
    /// </summary>
    public TimeSpan? ActiveTimeThreshold { get; init; }
}