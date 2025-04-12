using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models.Inputs;
public abstract class SensorInput(int id, string name): IInput
{
    private const string SensorInputTypeName = "Sensor";

    public int Id => id;

    public string DeviceType { get; } = SensorInputTypeName;

    public string Name => name;
}
