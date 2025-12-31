using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Abstractions;

public class VirtualInputActivation(VirtualInput virtualInput, InputPhase phase)
    : InputActivation(virtualInput, phase)
{
}
