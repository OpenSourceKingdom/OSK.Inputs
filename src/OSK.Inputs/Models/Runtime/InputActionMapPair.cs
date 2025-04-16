using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Runtime;
public struct InputActionMapPair(IInput input, InputActionMap actionMap)
{
    public int InputId => input.Id;

    public string InputName => input.Name;

    public string ActionKey => actionMap.ActionKey;

    public IInput DeviceInput => input;

    public InputPhase TriggerPhase => actionMap.InputPhase;
}
