using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;

public struct InputActionMap(string actionKey, int inputKey, ISet<InputPhase> triggerPhases)
{
    public readonly string ActionKey => actionKey;

    public readonly int InputId => inputKey;

    public readonly ISet<InputPhase> TriggerPhases => triggerPhases;
}
