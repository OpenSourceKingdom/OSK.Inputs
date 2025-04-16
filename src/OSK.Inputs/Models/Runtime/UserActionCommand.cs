using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Events;

namespace OSK.Inputs.Models.Runtime;
public class UserActionCommand(int userId, ActivatedInput activatedInput, InputAction inputAction)
{
    public int UserId => userId;

    public ActivatedInput ActivatedInput => activatedInput;

    public InputAction InputAction => inputAction;
}
