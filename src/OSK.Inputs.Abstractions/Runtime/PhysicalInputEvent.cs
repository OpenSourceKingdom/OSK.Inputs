using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public abstract class PhysicalInputEvent(RuntimeDeviceIdentifier deviceIdentifier, Input input, InputPhase phase)
    : InputEvent(input, phase)
{
    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
