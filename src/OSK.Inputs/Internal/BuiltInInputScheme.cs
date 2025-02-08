using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Internal;

internal class BuiltInInputScheme(string inputDefinitionName, string controllerName, string schemeName,
    IEnumerable<InputReceiverConfiguration> receiverConfigurations, bool isDefault) 
    : InputScheme(inputDefinitionName, controllerName, schemeName, receiverConfigurations, isDefault)
{
}
