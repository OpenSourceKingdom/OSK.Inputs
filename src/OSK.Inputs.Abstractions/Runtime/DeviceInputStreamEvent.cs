using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public abstract class DeviceInputStreamEvent(RuntimeDeviceIdentifier device, int inputId, InputPhase phase)
    : DeviceInputEvent(device, inputId, phase)
{
}
