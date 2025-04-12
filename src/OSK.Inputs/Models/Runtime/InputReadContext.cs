using System.Collections.Generic;

namespace OSK.Inputs.Models.Runtime;
public class InputReadContext(IEnumerable<InputActionCommand> actionSchemeMaps)
{
    public IEnumerable<InputActionCommand> ActionSchemeMaps => actionSchemeMaps;
}
