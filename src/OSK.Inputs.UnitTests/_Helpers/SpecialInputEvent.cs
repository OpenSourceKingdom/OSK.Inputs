using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.UnitTests._Helpers;

public class SpecialInputEvent(RuntimeDeviceIdentifier deviceIdentifier, IDeviceInput p, InputPhase phase)
    : DeviceInputEvent(deviceIdentifier, p, phase)
{
}
