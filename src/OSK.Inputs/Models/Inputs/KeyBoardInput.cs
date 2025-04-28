using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Inputs;
public class KeyBoardInput : DigitalInput, IKeyboardInput
{
    #region Variables

    public string Symbol { get; }

    #endregion

    #region Constructors

    public KeyBoardInput(int id, string name)
    : this(id, name, name)
    {
    }

    public KeyBoardInput(int id, string name, string symbol)
        : base(id, name, Keyboard.KeyboardName.Name)
    {
        Symbol = symbol;
    }

    #endregion
}
