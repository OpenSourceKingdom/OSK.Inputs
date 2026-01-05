using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestInput(int id): Input(InputDeviceType.Generic, id), IDeviceInput
{
}
