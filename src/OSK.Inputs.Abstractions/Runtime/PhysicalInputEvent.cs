using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public abstract class PhysicalInputEvent(RuntimeDeviceIdentifier deviceIdentifier, PhysicalInput input, InputPhase phase)
    : InputEvent(input, phase)
{
    public new PhysicalInput Input => input;

    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
