using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// An input event occurs when a user interacts with an input from a device. This input is then processed by the input system
/// and actions are triggered when they meet the needed conditions to do so.
/// </summary>
/// <param name="input">The input that triggered this event</param>
/// <param name="phase">The phase of the input</param>
public abstract class InputEvent(IInput input, InputPhase phase)
{
    #region Variables

    /// <summary>
    /// The input that triggered this event
    /// </summary>
    public IInput Input => input;

    /// <summary>
    /// The phase of the input
    /// </summary>
    public InputPhase Phase => phase;

    #endregion
}
