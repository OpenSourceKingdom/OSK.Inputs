using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestVirtualInput(params IDeviceInput[] inputs) : VirtualInput(InputDeviceType.Generic, 1)
{
    public override bool Contains(IInput input)
    {
        return inputs.Any(p => p.Id == input.Id);
    }
}
