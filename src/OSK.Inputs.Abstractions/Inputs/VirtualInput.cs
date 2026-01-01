namespace OSK.Inputs.Abstractions.Inputs;

public abstract class VirtualInput(int id, string name, InputType inputType)
    : Input(id, name, inputType)
{
    public abstract bool Contains(Input input);
}
