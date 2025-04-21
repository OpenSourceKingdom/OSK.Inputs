using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;
public class InputReaderParameters(InputDeviceIdentifier deviceIdentifier, IEnumerable<IInput> inputs)
{
    public InputDeviceIdentifier DeviceIdentifier => deviceIdentifier;

    public IEnumerable<IInput> Inputs => inputs;
}
