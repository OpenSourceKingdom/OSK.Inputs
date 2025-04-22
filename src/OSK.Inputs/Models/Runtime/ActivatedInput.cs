using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;

public class ActivatedInput(int userId, InputDeviceName deviceName, InputActionMapPair inputActionMapPair,
    InputPhase triggeredPhase, InputPower inputPower, PointerInformation pointerInformation)
{
    #region Variables

    public int UserId => userId;

    public InputDeviceName DeviceName => deviceName;

    public IInput Input => inputActionMapPair.DeviceInput;

    public string ActionKey => inputActionMapPair.ActionKey;

    public InputPhase TriggeredPhase => triggeredPhase;

    public InputPower InputPower => inputPower;

    public PointerInformation PointerInformation => pointerInformation;

    #endregion
}
