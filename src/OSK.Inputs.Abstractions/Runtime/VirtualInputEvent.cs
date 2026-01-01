using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public class VirtualInputEvent(VirtualInput virtualInput, InputPhase phase)
    : InputEvent(virtualInput, phase)
{
}
