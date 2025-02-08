using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;

public struct InputActionMap(string actionKey, string inputKey, InputPhase inputPhase)
{
    public readonly string ActionKey => actionKey;

    public readonly string InputKey => inputKey;

    public readonly InputPhase InputPhase => inputPhase;
}
