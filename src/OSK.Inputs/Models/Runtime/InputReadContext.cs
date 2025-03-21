using System.Collections.Generic;

namespace OSK.Inputs.Models.Runtime;
public class InputReadContext(IEnumerable<InputActionSchemeMap> actionSchemeMaps)
{
    public IEnumerable<InputActionSchemeMap> ActionSchemeMaps => actionSchemeMaps;
}
