using System.Collections.Generic;

namespace OSK.Inputs.Models.Inputs;

public class CombinationInput(string name, params HardwareInput[] inputs): VirtualInput(name)
{
    public IEnumerable<HardwareInput> Inputs => inputs;
}
