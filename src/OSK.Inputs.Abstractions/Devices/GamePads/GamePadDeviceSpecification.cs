using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public abstract class GamePadDeviceSpecification : InputDeviceSpecification<GamePadInput>
{
    #region InputDeviceSpecification Overrides

    public override IReadOnlyCollection<IInput> GetInputs()
        => [.. Inputs.Select(i => (IInput)(i switch 
        {
            GamePadInput.LeftTrigger or GamePadInput.RightTrigger 
            or GamePadInput.LeftJoyStick or GamePadInput.RightJoyStick => new AnalogInput(InputDeviceType.GamePad, (int)i),
            _ => new DigitalInput(InputDeviceType.GamePad, (int)i),
        }))];

    #endregion

    #region Helpers

    protected abstract GamePadInput[] Inputs { get; }

    #endregion
}
