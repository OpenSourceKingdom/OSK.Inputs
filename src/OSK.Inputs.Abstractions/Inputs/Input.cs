namespace OSK.Inputs.Abstractions.Inputs;

public abstract class Input(int id, string name, InputType inputType)
{
    #region Variables

    public int Id => id;

    public string Name => name;

    public InputType InputType => inputType;

    #endregion
}
