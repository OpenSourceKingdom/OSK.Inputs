using System.Diagnostics.CodeAnalysis;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal class ProcessedInputEvent(ActiveInputActionMap? actionMap, InputEventContext? context)
{
    internal static ProcessedInputEvent NotTriggered = new(null, null);

    [MemberNotNullWhen(true, nameof(ActionMap), nameof(ActivationContext))]
    public bool Triggered => actionMap is not null;

    public ActiveInputActionMap? ActionMap => actionMap;

    public InputEventContext? ActivationContext => context;

    public void Execute()
    {
        if (Triggered)
        {
            ActionMap.Action.ActionExecutor(ActivationContext);
        }
    }
}
