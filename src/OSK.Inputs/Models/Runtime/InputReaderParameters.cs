using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;
public class InputReaderParameters(InputDeviceIdentifier controllerIdentifier, IEnumerable<IInput> inputs)
{
    public InputDeviceIdentifier ControllerIdentifier => controllerIdentifier;

    public IEnumerable<IInput> Inputs => inputs;
}
