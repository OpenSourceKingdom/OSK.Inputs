using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

public readonly struct TriggeredActionEvent(ActiveInputScheme activeScheme, DeviceInputActionMap actionMap, InputEventContext context)
{
    public ActiveInputScheme Scheme => activeScheme;

    public DeviceInputActionMap ActionMap => actionMap;

    public InputEventContext ActivationContext => context;

    public void Execute()
        => ActionMap.Action.Execute(ActivationContext);
}
