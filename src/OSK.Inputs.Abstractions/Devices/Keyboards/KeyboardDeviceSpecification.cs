using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public abstract class KeyboardDeviceSpecification : InputDeviceSpecification<KeyboardInput>
{
    #region InputDeviceSpecification Overrides

    public override IReadOnlyCollection<IInput> GetInputs()
        => [.. StandardKeys.Select(key => (Input) new KeyboardKeyInput(key)).Concat(Combinations)];

    #endregion

    #region Helpers

    protected abstract KeyboardInput[] StandardKeys { get; }

    protected abstract KeyboardCombination[] Combinations { get; }

    #endregion
}
