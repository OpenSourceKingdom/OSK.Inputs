using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.UnitTests._Helpers;

public class TestDeviceFamily
{
    public static InputDeviceFamily Identity1 = new("Test 1", InputDeviceType.Generic);
    public static InputDeviceFamily Identity2 = new("Test 2", InputDeviceType.Generic);
    public static InputDeviceFamily Identity3 = new("Test 3", InputDeviceType.Generic);
    public static InputDeviceFamily Identity4 = new("Test 4", InputDeviceType.Generic);
}
