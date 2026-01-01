using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public abstract class InputEvent(Input input, InputPhase phase)
{
    public Input Input => input;

    public InputPhase Phase => phase;
}
