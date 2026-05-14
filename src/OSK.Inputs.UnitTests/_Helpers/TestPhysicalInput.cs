using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestPhysicalInput(int id): DeviceInput(InputDeviceType.Generic, id)
{
}
