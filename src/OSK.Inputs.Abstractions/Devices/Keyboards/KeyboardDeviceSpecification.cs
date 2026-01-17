using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public abstract class KeyboardDeviceSpecification : InputDeviceSpecification<KeyboardInput>
{
    #region InputDeviceSpecification Overrides

    public override IReadOnlyCollection<DeviceInput> GetInputs()
        => [.. StandardKeys.Select(key => (DeviceInput) new KeyboardKeyInput(key))];

    #endregion

    #region Helpers

    protected abstract KeyboardInput[] StandardKeys { get; }

    #endregion
}
