using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal class InputPowerState(PhysicalInput input): PhysicalInputState(input)
{
    public int TapCount { get; set; }

    public required InputIntensity[] InputPowers { get; set; }
}
