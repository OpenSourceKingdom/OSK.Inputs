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
    /// The amount of time an input phase can be over before it is considered completely over.
    /// i.e. if an interaction starts and ends and starts within this time frame it is considered a 'tap'
    /// rather than a completely new event
    /// </summary>
    public TimeSpan? TapDelayTime { get; init; }

    /// <summary>
    /// The amount of time an input phase can be 'Start'ed before the input system interpets the phase as <see cref="InputPhase.Active"/>
    /// </summary>
    public TimeSpan? StartPhaseDelayBeforeActive { get; init; }
}