using System.Diagnostics.CodeAnalysis;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal readonly struct ProcessedInputEvent(InputActionMap? actionMap, InputEventContext? context)
{
    internal static ProcessedInputEvent NotTriggered = new ProcessedInputEvent(null, null);

    [MemberNotNullWhen(true, nameof(ActionMap), nameof(ActivationContext))]
    public bool Triggered => actionMap is not null;

    public InputActionMap? ActionMap => actionMap;

    public InputEventContext? ActivationContext => context;

    public void Execute()
    {
        if (Triggered)
        {
            ActionMap?.Action.ActionExecutor(ActivationContext);
        }
    }
}
