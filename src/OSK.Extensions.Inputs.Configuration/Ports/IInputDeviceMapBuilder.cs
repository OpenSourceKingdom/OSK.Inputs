using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Extensions.Inputs.Configuration.Ports;

public interface IInputDeviceMapBuilder
{
    IInputDeviceMapBuilder WithInputMap(int inputId, string actionName);
}
