using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models.Inputs;
public class TouchInput(int touchCount): SensorInput($"T{touchCount}")
{
    public int TouchCount => touchCount;
}
