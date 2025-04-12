using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Inputs;
public class KeyboardCombination(int id, string name, string symbol, params KeyBoardInput[] inputs) 
    : CombinationInput(id, name, Keyboard.KeyboardName.Name, inputs)
{
    public string Symbol => symbol;
}
