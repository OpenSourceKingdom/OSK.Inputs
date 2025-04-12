using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Internal;

internal class BuiltInInputScheme(string inputDefinitionName, string deviceName, string schemeName, bool isDefault,
    IEnumerable<InputActionMap> actionMaps) 
    : InputScheme(inputDefinitionName, deviceName, schemeName, isDefault, actionMaps)
{
}
