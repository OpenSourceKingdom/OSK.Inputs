using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestInput(int id): DeviceInput(InputDeviceType.Generic, id)
{
}
