namespace OSK.Inputs.Abstractions;

public abstract class InputActivation(Input input, InputPhase phase)
{
    public Input Input => input;

    public InputPhase Phase => phase;
}
