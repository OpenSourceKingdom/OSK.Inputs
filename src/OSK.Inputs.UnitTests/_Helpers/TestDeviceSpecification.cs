using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestDeviceSpecification(InputDeviceFamily deviceFamily, params DeviceInput[] inputs) : InputDeviceSpecification
{
    public override InputDeviceFamily DeviceFamily => deviceFamily;

    public override IReadOnlyCollection<DeviceInput> GetInputs() => inputs;
}
