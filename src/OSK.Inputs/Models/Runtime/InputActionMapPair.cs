using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;
public struct InputActionMapPair(IInput input, InputActionMap actionMap)
{
    public int InputId => input.Id;

    public string InputName => input.Name;

    public string ActionKey => actionMap.ActionKey;

    public IInput DeviceInput => input;

    public ISet<InputPhase> TriggerPhases => actionMap.TriggerPhases;
}
