using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestDeviceSpecification(InputDeviceIdentity deviceIdentity, params Input[] inputs) : InputDeviceSpecification
{
    public override InputDeviceIdentity DeviceIdentity => deviceIdentity;

    public override IReadOnlyCollection<Input> Inputs => inputs;
}
