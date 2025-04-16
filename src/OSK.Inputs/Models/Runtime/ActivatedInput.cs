using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;

public class ActivatedInput(int userId, InputActionMapPair inputActionMapPair,
    InputPhase triggeredPhase, InputPower inputPower, PointerInformation pointerInformation)
{
    #region Variables

    public int UserId => userId;

    public IInput Input => inputActionMapPair.DeviceInput;

    public string ActionKey => inputActionMapPair.ActionKey;

    public InputPhase TriggeredPhase => triggeredPhase;

    public InputPower InputPower => inputPower;

    public PointerInformation PointerInformation => pointerInformation;

    #endregion
}
