using System;

namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// The input phase is the state of a given input. These values represent:
/// <para>- Start: An input has just been activated</para>
/// <para>- Hold: An input has been continously held since being activated</para>
/// <para>- Translation: An input has been moved (i.e. touch/cursor) while it was activated</para>
/// <para>- End: An input has been released</para>
/// </summary>
[Flags]
public enum InputPhase
{
    /// <summary>
    /// The input has just been activated
    /// </summary>
    Start = 1,

    /// <summary>
    /// The input has been continously held since being activated
    /// </summary>
    Hold = 2,

    /// <summary>
    /// The input has been moved (i.e. touch/cursor) while it was activated
    /// </summary>
    Translation = 4,

    /// <summary>
    /// The input has been released
    /// </summary>
    End = 8
}
