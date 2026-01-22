using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal abstract class DeviceInputState(DeviceInput input): InputState<DeviceInput>(input)
{
    public VirtualInput? LinkedVirtualInput { get; set; }
}
