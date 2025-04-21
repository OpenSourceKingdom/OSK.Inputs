using System;
using System.Threading.Tasks;
using OSK.Inputs.Models.Events;

namespace OSK.Inputs.Models.Configuration;

public readonly struct InputAction(string actionKey, Func<InputActivationEvent, ValueTask> actionExecutor,
    string? description)
{
    public string ActionKey => actionKey;

    public string? Description => description;

    public Func<InputActivationEvent, ValueTask> ActionExecutor => actionExecutor;
}
