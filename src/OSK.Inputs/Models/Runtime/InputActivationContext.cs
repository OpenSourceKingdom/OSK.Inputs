using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSK.Inputs.Models.Events;

namespace OSK.Inputs.Models.Runtime;

public delegate ValueTask InputActivationDelegate(InputActivationEvent @event);

public class InputActivationContext(IServiceProvider serviceProvider, IEnumerable<UserActionCommand> activatedInputs)
{
    #region Helpers

    public async ValueTask ExecuteCommandsAsync(Func<InputActivationDelegate, InputActivationEvent, ValueTask>? middleware)
    {
        foreach (var command in activatedInputs)
        {
            InputActivationDelegate executionDelegate =
                @event => command.InputAction.ActionExecutor(@event);

            var activationEvent = new InputActivationEvent(serviceProvider, command.UserId, command.ActivatedInput, command.InputAction);
            if (middleware != null)
            {
                await middleware(executionDelegate, activationEvent);
            }
            else
            {
                await executionDelegate(activationEvent);
            }
        }
    }

    #endregion
}
