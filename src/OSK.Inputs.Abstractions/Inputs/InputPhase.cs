using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// The phase of an input represents a logical state and is utilized when performing operations for <see cref="InputAction"/>
/// </summary>
public enum InputPhase
{
    /// <summary>
    /// The input was just initiated and has started activity
    /// </summary>
    Start,

    /// <summary>
    /// The input has been active for a given period of time
    /// </summary>
    Active,

    /// <summary>
    /// The input has just ended and activity stopped
    /// </summary>
    End
}
