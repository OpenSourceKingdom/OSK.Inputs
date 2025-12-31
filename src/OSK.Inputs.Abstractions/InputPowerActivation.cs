using System;
using System.Collections.Generic;

namespace OSK.Inputs.Abstractions;

public class InputPowerActivation(RuntimeDeviceIdentifier device, Input input, InputPhase phase, InputPower[] inputPowers)
    : DeviceInputActivation(device, input, phase)
{
    public int TotalPoweredAxis => inputPowers.Length;

    public IEnumerable<InputPower> GetInputPowers() => inputPowers;

    public InputPower this[int index]
        => 
        index < 0 || index >= inputPowers.Length
            ? throw new IndexOutOfRangeException(nameof(index))
            : inputPowers[index];
}
