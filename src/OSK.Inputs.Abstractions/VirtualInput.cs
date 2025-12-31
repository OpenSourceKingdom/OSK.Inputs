namespace OSK.Inputs.Abstractions;

public abstract class VirtualInput(int id, string name, InputType inputType)
    : Input(id, name, inputType)
{
    public abstract bool Contains(Input input);
}
