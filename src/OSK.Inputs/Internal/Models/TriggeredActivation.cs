using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Internal.Models;

public readonly struct TriggeredActivation(ActiveInputScheme activeScheme, InputActionMap actionMap, InputActivationContext context)
{
    public ActiveInputScheme Scheme => activeScheme;

    public InputActionMap ActionMap => actionMap;

    public InputActivationContext ActivationContext => context;
}
