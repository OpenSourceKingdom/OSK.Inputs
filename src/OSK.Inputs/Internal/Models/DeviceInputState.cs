using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal abstract class DeviceInputState(IDeviceInput input): InputState<IDeviceInput>(input)
{

}
