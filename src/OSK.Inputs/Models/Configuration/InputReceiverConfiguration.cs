using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;

public class InputReceiverConfiguration(string inputReceiverName, IEnumerable<InputActionMap> inputMaps)
{
    public string InputReceiverName => inputReceiverName;

    public IReadOnlyCollection<InputActionMap> InputMaps { get; } = inputMaps.ToArray();
}
