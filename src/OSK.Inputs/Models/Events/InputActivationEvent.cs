using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Models.Events;
public class InputActivationEvent(IServiceProvider serviceProvider, int userId, ActivatedInput input, InputAction inputAction)
{
    public IServiceProvider Services => serviceProvider;

    public int UserId => userId;

    public ActivatedInput Input => input;

    public InputAction InputAction => inputAction;
}
