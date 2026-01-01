using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestIdentity
{
    public static InputDeviceIdentity Identity1 = new InputDeviceIdentity("Test 1", "test");
    public static InputDeviceIdentity Identity2 = new InputDeviceIdentity("Test 2", "test");
    public static InputDeviceIdentity Identity3 = new InputDeviceIdentity("Test 3", "test");
    public static InputDeviceIdentity Identity4 = new InputDeviceIdentity("Test 4", "test");
}
