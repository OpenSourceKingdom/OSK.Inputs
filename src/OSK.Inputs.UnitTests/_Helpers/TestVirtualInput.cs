using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestVirtualInput(params DeviceInput[] inputs) : VirtualInput(InputDeviceType.Generic, inputs)
{
}
