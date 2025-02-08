using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;

public class ActivatedInput(string inputReceiverName, IInput input, string actionKey,
    InputPhase inputPhase, InputPower inputPower)
{
    #region Variables

    public string InputReceiverName => inputReceiverName;

    public IInput Input => input;

    public string ActionKey => actionKey;

    public InputPhase TriggeredPhase => inputPhase;

    public InputPower InputPower => inputPower;

    #endregion
}
