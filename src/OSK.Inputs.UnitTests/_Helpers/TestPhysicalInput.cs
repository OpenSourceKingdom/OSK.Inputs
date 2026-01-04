using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestPhysicalInput(int id): PhysicalInput("Test", id, InputType.Analog)
{
}
