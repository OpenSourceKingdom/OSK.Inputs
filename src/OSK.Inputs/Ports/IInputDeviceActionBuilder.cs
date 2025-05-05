using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Ports;
public interface IInputDeviceActionBuilder<TInput>
    where TInput: IInput
{
    IInputDeviceActionBuilder<TInput> AssignInput(TInput input, IEnumerable<InputPhase> triggerPhases, string actionKey);
}
