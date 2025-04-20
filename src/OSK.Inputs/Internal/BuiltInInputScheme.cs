using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Internal;

internal class BuiltInInputScheme(string inputDefinitionName, string schemeName, bool isDefault,
    IEnumerable<InputDeviceActionMap> deviceActionMaps) 
    : InputScheme(inputDefinitionName, schemeName, isDefault, deviceActionMaps)
{
}
