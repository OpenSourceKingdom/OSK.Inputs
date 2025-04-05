using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Runtime;
public struct InputActionSchemeMap(string inputName, InputPhase triggerPhase, InputActionOptions actionOptions)
{
    public string InputName => inputName;

    public InputPhase TriggerPhase => triggerPhase;

    public InputActionOptions ActionOptions => actionOptions;
}
