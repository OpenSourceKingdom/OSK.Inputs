using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models.Inputs;
public static class SensorInputs
{
    public static readonly AccelerometerInput Accelerometer = new AccelerometerInput();

    public static readonly TouchInput OneTouch = new TouchInput(1);
    public static readonly TouchInput TwoTouch = new TouchInput(2);
    public static readonly TouchInput ThreeTouch = new TouchInput(3);
    public static readonly TouchInput FourTouch = new TouchInput(4);
    public static readonly TouchInput FiveTouch = new TouchInput(5);
}
