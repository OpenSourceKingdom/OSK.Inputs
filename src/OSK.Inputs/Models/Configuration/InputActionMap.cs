using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;

public struct InputActionMap(string actionKey, int inputKey, InputPhase inputPhase)
{
    public readonly string ActionKey => actionKey;

    public readonly int InputId => inputKey;

    public readonly InputPhase InputPhase => inputPhase;
}
