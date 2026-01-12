using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public abstract class GamePadDeviceSpecification : InputDeviceSpecification<GamePadInput>
{
    #region InputDeviceSpecification Overrides

    public override IReadOnlyCollection<IInput> GetInputs()
        => [.. Inputs.Select(i => i.ToInput()))];

    #endregion

    #region Helpers

    protected abstract GamePadInput[] Inputs { get; }

    #endregion
}
