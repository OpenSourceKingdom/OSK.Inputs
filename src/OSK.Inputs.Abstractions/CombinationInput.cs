using System.Linq;

namespace OSK.Inputs.Abstractions;

public class CombinationInput(int id, params DeviceInput[] deviceInputs)
    : VirtualInput(id, string.Join(".", deviceInputs.Select(input => input.Name)), InputType.Digital)
{
    #region Variables

    public DeviceInput[] DeviceInputs => deviceInputs;

    #endregion

    #region VirtualInputs Overrides

    public override bool Contains(Input input)
    {
        return input is DeviceInput deviceInput && deviceInputs.Contains(deviceInput);
    }

    #endregion
}
