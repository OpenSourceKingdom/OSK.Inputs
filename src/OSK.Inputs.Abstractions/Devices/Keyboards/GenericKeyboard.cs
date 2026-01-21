using System;
using System.Linq;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class GenericKeyboard : KeyboardDeviceSpecification
{
    #region KeyboardDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => InputDeviceFamily.GenericKeyboard;

    protected override KeyboardInput[] StandardKeys { get; }
        = [.. Enum.GetValues(typeof(KeyboardInput)).Cast<KeyboardInput>()];

    #endregion
}
