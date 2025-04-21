using System;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Models.Events;
public class InputActivationEvent(IServiceProvider serviceProvider, ActivatedInput input)
{
    public IServiceProvider Services => serviceProvider;

    public ActivatedInput Input => input;
}
