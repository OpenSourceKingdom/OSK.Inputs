using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public class SensorControllerConfigurator(Type inputReaderType): InputControllerConfigurator(SensorControllerName, inputReaderType)
{
    public static readonly InputControllerName SensorControllerName = new InputControllerName("SensorController");

    public SensorControllerConfigurator WithTouch(TouchInput touchInput, Action<DigitalInputOptions>? optionConfiguration)
    {
        AddInputConfiguration(touchInput, optionConfiguration);
        return this;
    }

    public SensorControllerConfigurator WithAccelerometer(Action<AccelerometerOptions>? optionConfiguration)
    {
        AddInputConfiguration(SensorInputs.Accelerometer, optionConfiguration);
        return this;
    }
}
