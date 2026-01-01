using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestDeviceSpecification(params Input[] inputs) : InputDeviceSpecification
{
    public override InputDeviceIdentity DeviceIdentity => TestIdentity.Identity1;

    public override IReadOnlyCollection<Input> Inputs => inputs;
}
