using System;
using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public class InputPowerEvent(RuntimeDeviceIdentifier device, PhysicalInput input, InputPhase phase, InputPower[] inputPowers)
    : PhysicalInputEvent(device, input, phase)
{
    public int TotalPoweredAxis => inputPowers.Length;

    public IEnumerable<InputPower> GetInputPowers() => inputPowers;

    public InputPower this[int index]
        => 
        index < 0 || index >= inputPowers.Length
            ? throw new IndexOutOfRangeException(nameof(index))
            : inputPowers[index];
}
