using System;
using System.Linq;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class GenericGamePad : GamePadDeviceSpecification
{
    #region GamePadDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => InputDeviceFamily.GenericGamePad;

    protected override GamePadInput[] Inputs { get; }
        = [.. Enum.GetValues(typeof(GamePadInput)).Cast<GamePadInput>()];

    #endregion
}
