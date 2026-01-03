using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.UnitTests._Helpers;

public class SpecialInputEvent(RuntimeDeviceIdentifier deviceIdentifier, PhysicalInput p, InputPhase phase)
    : PhysicalInputEvent(deviceIdentifier, p, phase)
{
}
