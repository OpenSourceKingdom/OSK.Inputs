using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;
public class InputReadContext(IEnumerable<InputActionSchemeMap> actionSchemeMaps)
{
    public IEnumerable<InputActionSchemeMap> ActionSchemeMaps => actionSchemeMaps;
}
