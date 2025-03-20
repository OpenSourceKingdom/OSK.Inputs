using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;
public class InputReaderParameters(InputControllerIdentifier controllerIdentifier, IEnumerable<IInput> inputs)
{
    public InputControllerIdentifier ControllerIdentifier => controllerIdentifier;

    public IEnumerable<IInput> Inputs => inputs;
}
