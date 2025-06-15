using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;

/// <summary>
/// Represents a finalized user action command after input information has been received and processed
/// </summary>
/// <param name="userId">The user id the command was executed by</param>
/// <param name="activatedInput">The input that was activated</param>
/// <param name="inputAction">The input action that was triggered</param>
public class UserActionCommand(int userId, ActivatedInput activatedInput, InputAction inputAction)
{
    /// <summary>
    /// The user id that the command was generated for
    /// </summary>
    public int UserId => userId;

    /// <summary>
    /// The input that triggered the command
    /// </summary>
    public ActivatedInput ActivatedInput => activatedInput;

    /// <summary>
    /// The related input action that is associated with the input
    /// </summary>
    public InputAction InputAction => inputAction;
}
