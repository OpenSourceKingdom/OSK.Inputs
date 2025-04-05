using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models.Inputs;
public class KeyboardCombination(string name, params KeyBoardInput[] inputs) : CombinationInput(name, inputs)
{
}
