using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestPhysicalInput(int id): PhysicalInput(TestIdentity.Identity1, id, "Abc", InputType.Analog)
{
}
