using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Inputs;
public class MouseScrollInput(int id): AnalogInput(id, "Scroll Wheel", Mouse.MouseName.Name)
{
}
