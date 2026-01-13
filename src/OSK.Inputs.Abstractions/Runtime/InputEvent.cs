using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// An input event occurs when a user interacts with an input from a device. This input is then processed by the input system
/// and actions are triggered when they meet the needed conditions to do so.
/// </summary>
/// <param name="inputId">The input id that triggered this event</param>
/// <param name="phase">The phase of the input</param>
public abstract class InputEvent(int inputId, InputPhase phase)
{
    #region Variables

    /// <summary>
    /// The input id that triggered this event
    /// </summary>
    public int InputId => inputId;

    /// <summary>
    /// The phase of the input
    /// </summary>
    public InputPhase Phase => phase;

    #endregion
}
