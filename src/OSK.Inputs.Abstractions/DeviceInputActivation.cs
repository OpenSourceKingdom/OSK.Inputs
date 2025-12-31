using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Abstractions;

public abstract class DeviceInputActivation(RuntimeDeviceIdentifier deviceIdentifier, Input input, InputPhase phase)
    : InputActivation(input, phase)
{
    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
