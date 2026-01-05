using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Mice;

public abstract class MouseDeviceSpecification: InputDeviceSpecification<MouseInput>
{
    #region InputDeviceSpecification Overrides

    public override IReadOnlyCollection<IInput> GetInputs()
        => [.. Inputs.Select(input => new MouseDeviceInput(input))];

    #endregion

    #region Helpers

    protected abstract MouseInput[] Inputs { get; }

    #endregion
}
