using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestVirtualInput(params PhysicalInput[] inputs) : VirtualInput("Test", 1, InputType.Digital)
{
    public override bool Contains(Input input)
    {
        return inputs.Any(p => p.Id == input.Id);
    }
}
