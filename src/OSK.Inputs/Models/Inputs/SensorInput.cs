using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models.Inputs;
public abstract class SensorInput(string name): IInput
{
    public string Name => name;
}
