using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models.Runtime;
public class ActivatedPointerInput(ActivatedInput input, IEnumerable<PointerInformation> pointerInformation)
    : ActivatedInput(input.ControllerIdentifier, input.Input, input.ActionKey, input.TriggeredPhase, input.InputPower)
{
    public IEnumerable<PointerInformation> Pointers => pointerInformation;
}
