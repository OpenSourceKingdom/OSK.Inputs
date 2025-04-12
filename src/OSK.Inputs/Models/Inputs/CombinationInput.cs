using System.Collections.Generic;

namespace OSK.Inputs.Models.Inputs;

public class CombinationInput(int id, string name, string deviceType, params HardwareInput[] inputs)
    : VirtualInput(id, name, deviceType)
{
    public IEnumerable<HardwareInput> Inputs => inputs;
}
