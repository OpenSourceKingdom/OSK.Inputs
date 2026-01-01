using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public class VirtualInputEvent(VirtualInput virtualInput, InputPhase phase)
    : InputEvent(virtualInput, phase)
{
}
