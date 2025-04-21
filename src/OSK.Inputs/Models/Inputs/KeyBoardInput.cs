using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Inputs;
public class KeyBoardInput(int id, string name, string symbol) : DigitalInput(id, name, Keyboard.KeyboardName.Name), IKeyboardInput
{
    public string Symbol => symbol;
}
