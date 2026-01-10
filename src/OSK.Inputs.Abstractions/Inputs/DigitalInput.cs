using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// An input that provides ON/OFF or 0|1 input values
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">A unique id for the input</param>
public class DigitalInput(InputDeviceType deviceType, int id): Input(deviceType, id), IDeviceInput
{
}
