using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

public readonly struct TriggeredActionEvent(ActiveInputScheme activeScheme, InputActionMap actionMap, InputEventContext context)
{
    public ActiveInputScheme Scheme => activeScheme;

    public InputActionMap ActionMap => actionMap;

    public InputEventContext ActivationContext => context;

    public void Execute()
        => ActionMap.Action.ActionExecutor(ActivationContext);
}
