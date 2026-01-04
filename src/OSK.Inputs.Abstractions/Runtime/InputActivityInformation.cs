using System;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Provides extra details to the activity of a particular input
/// </summary>
public class InputActivityInformation
{
    /// <summary>
    /// How many times the input was used over a time period. The particular time utilized to determine taps 
    /// can be adjusted using <see cref="InputProcessorConfiguration"/>
    /// </summary>
    public int? TapCount { get; set; }

    /// <summary>
    /// The duration the input has been interacted with
    /// </summary>
    public TimeSpan Duration { get; set; }
}
