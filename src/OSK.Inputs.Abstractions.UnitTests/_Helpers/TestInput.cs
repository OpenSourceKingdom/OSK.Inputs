using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestInput(int id): PhysicalInput(TestIdentity.Identity1, id, "Test", InputType.Digital)
{
}
