using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;

public class ActivatedInput(InputControllerIdentifier controllerIdentifier, IInput input, string actionKey,
    InputPhase inputPhase, InputPower inputPower)
{
    #region Variables

    public InputControllerIdentifier ControllerIdentifier => controllerIdentifier;

    public IInput Input => input;

    public string ActionKey => actionKey;

    public InputPhase TriggeredPhase => inputPhase;

    public InputPower InputPower => inputPower;

    #endregion
}
