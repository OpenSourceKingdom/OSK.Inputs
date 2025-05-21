using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;

public class ActivatedInput(InputDeviceName deviceName, IInput input,
    InputPhase triggeredPhase, InputPower inputPower, PointerInformation pointerInformation)
{
    #region Variables

    public InputDeviceName DeviceName => deviceName;

    public IInput Input => input;

    public InputPhase TriggeredPhase => triggeredPhase;

    public InputPower InputPower => inputPower;

    public PointerInformation PointerInformation { get; private set; } = pointerInformation;

    #endregion
}
