using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;

public class InputScheme(string inputDefinitionName, string controllerName, string schemeName,
    IEnumerable<InputReceiverConfiguration> receiverConfigurations, bool isDefault)
{
    public string InputDefinitionName => inputDefinitionName;

    public string ControllerName => controllerName;

    public string SchemeName => schemeName;

    public bool IsDefault => isDefault;

    public IReadOnlyCollection<InputReceiverConfiguration> ReceiverConfigurations { get; } = receiverConfigurations.ToArray();
}
