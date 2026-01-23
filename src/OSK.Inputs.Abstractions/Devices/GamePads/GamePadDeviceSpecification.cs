using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

/// <summary>
/// A device specification that defines input for GamePad devices
/// </summary>
public abstract class GamePadDeviceSpecification : InputDeviceSpecification<GamePadInput>
{
    #region InputDeviceSpecification Overrides

    public override IReadOnlyCollection<DeviceInput> GetInputs()
        => [.. Inputs.Select(i => i.ToInput())];

    #endregion

    #region Helpers

    protected abstract GamePadInput[] Inputs { get; }

    #endregion
}
