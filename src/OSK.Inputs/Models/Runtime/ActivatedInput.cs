using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;

public class ActivatedInput(int playerId, string inputReceiverName, IInput input, string actionKey,
    InputPhase inputPhase, InputPower inputPower, PointerTranslation? pointerTranslation = null)
{
    #region Variables

    public int PlayerId => playerId;

    public string InputReceiverName => inputReceiverName;

    public IInput Input => input;

    public string ActionKey => actionKey;

    public InputPhase TriggeredPhase => inputPhase;

    public InputPower InputPower => inputPower;

    public PointerTranslation PointerTranslation => pointerTranslation ?? PointerTranslation.None;

    #endregion
}
