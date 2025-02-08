using System;
using System.Collections.Generic;

namespace OSK.Inputs.Models.Runtime;

public class InputActivationContext(IServiceProvider serviceProvider, IEnumerable<ActivatedInput> activatedInputs)
{
    public IServiceProvider Services => serviceProvider;

    public IEnumerable<ActivatedInput> ActivatedInputs => activatedInputs;
}
