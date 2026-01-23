using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestDeviceSpecification(params DeviceInput[] inputs) : InputDeviceSpecification
{
    public override InputDeviceFamily DeviceFamily => TestDeviceFamily.Identity1;

    public override IReadOnlyCollection<DeviceInput> GetInputs() => inputs;
}
