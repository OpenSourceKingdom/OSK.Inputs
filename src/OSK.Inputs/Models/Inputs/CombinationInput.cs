using System.Collections.Generic;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Inputs;

public class CombinationInput(string name, IEnumerable<HardwareInput> inputs, CombinationInputOptions options): VirtualInput<CombinationInputOptions>(name, options)
{
    public IEnumerable<HardwareInput> Inputs => inputs;
}
