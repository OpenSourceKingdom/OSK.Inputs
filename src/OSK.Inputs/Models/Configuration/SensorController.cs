﻿using System;
using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;
public class SensorController(Type inputReaderType): InputDevice(SensorControllerName, inputReaderType)
{
    #region Static

    public static readonly InputDeviceName SensorControllerName = new InputDeviceName("SensorController");

    public static readonly AccelerometerInput Accelerometer = new AccelerometerInput(20);

    public static readonly TouchInput OneTouch = new TouchInput(1);
    public static readonly TouchInput TwoTouch = new TouchInput(2);
    public static readonly TouchInput ThreeTouch = new TouchInput(3);
    public static readonly TouchInput FourTouch = new TouchInput(4);
    public static readonly TouchInput FiveTouch = new TouchInput(5);

    #endregion

    #region InputDevice Overrides

    public override IEnumerable<IInput> AllInputs { get; } = [
        OneTouch, TwoTouch, ThreeTouch, FourTouch, FiveTouch,
        Accelerometer
    ];

    #endregion
}
