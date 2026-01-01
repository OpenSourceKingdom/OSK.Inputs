using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestVirtualInput(params PhysicalInput[] inputs) : VirtualInput(1, "Hello", InputType.Digital)
{
    public override bool Contains(Input input)
    {
        return inputs.Any(p => p.Id == input.Id);
    }
}
