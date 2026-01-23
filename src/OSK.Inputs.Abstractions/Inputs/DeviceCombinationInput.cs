using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// A virtual input that uses a collection of <see cref="DeviceInput"/>s
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="deviceInputs">The inputs this will use</param>
public class DeviceCombinationInput(InputDeviceType deviceType, params DeviceInput[] deviceInputs)
    : VirtualInput(deviceType, deviceInputs)
{
    public IEnumerable<DeviceInput> GetDeviceInputs()
        => GetLinkedInputs().OfType<DeviceInput>();
}
