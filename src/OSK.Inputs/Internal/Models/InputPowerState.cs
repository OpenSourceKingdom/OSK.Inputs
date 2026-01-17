using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal class InputPowerState(DeviceInput input): DeviceInputState(input)
{
    public int TapCount { get; set; }

    public required InputIntensity[] InputPowers { get; set; }
}
