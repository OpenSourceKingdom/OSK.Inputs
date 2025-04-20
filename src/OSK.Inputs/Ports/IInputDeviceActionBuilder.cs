using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Ports;
public interface IInputDeviceActionBuilder
{
    IInputDeviceActionBuilder AssignInput(IInput input, InputPhase inputPhase, string actionKey);
}
