using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestDeviceSpecification(params Input[] inputs) : InputDeviceSpecification
{
    public override InputDeviceFamily DeviceFamily => TestDeviceFamily.Identity1;

    public override IReadOnlyCollection<IInput> GetInputs() => inputs;
}
