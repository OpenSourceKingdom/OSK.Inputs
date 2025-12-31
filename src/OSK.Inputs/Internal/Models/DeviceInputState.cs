using OSK.Inputs.Abstractions;

namespace OSK.Inputs.Internal.Models;

internal class DeviceInputState: InputState
{
    public required int[] VirtualInputIds { get; set; }
}
