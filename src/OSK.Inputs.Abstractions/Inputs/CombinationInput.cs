using System.Linq;

namespace OSK.Inputs.Abstractions.Inputs;

public class CombinationInput(int id, params PhysicalInput[] deviceInputs)
    : VirtualInput(id, string.Join(".", deviceInputs.Select(input => input.Name)), InputType.Digital)
{
    #region Variables

    public PhysicalInput[] DeviceInputs => deviceInputs;

    #endregion

    #region VirtualInputs Overrides

    public override bool Contains(Input input)
    {
        return input is PhysicalInput deviceInput && deviceInputs.Contains(deviceInput);
    }

    #endregion
}
