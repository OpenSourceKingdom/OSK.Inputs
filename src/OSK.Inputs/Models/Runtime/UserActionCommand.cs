using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;
public class UserActionCommand(int userId, ActivatedInput activatedInput, InputAction inputAction)
{
    public int UserId => userId;

    public ActivatedInput ActivatedInput => activatedInput;

    public InputAction InputAction => inputAction;
}
