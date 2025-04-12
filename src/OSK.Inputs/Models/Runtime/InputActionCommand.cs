using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Runtime;
public struct InputActionCommand(IInput input, InputActionMap actionMap, InputActionOptions actionOptions)
{
    public string InputName => input.Name;

    public string ActionKey => actionMap.ActionKey;

    public IInput DeviceInput => input;

    public InputPhase TriggerPhase => actionMap.InputPhase;

    public InputActionOptions ActionOptions => actionOptions;
}
