using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestPhysicalInput(int id): Input(InputDeviceType.Generic, id), IDeviceInput
{
}
