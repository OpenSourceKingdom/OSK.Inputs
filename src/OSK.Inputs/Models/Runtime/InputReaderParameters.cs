using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;
public class InputReaderParameters(InputControllerIdentifier controllerIdentifier, IEnumerable<InputConfiguration> inputConfigurations)
{
    public InputControllerIdentifier ControllerIdentifier => controllerIdentifier;

    public IEnumerable<InputConfiguration> InputCnfigurations => inputConfigurations;
}
