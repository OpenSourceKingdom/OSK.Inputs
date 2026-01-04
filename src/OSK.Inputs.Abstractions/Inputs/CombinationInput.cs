using System.Linq;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// A virtual input that uses a collection of <see cref="PhysicalInput"/>s
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">A unique id for the combination</param>
/// <param name="deviceInputs">The inputs this will use</param>
public class CombinationInput(string deviceType, int id, params PhysicalInput[] deviceInputs)
    : VirtualInput(deviceType, id, InputType.Digital)
{
    #region Variables

    /// <summary>
    /// The collection of <see cref="PhysicalInput"/>s this references to be triggered
    /// </summary>
    public PhysicalInput[] DeviceInputs => deviceInputs;

    #endregion

    #region VirtualInputs Overrides

    /// <inheritdoc/>
    public override bool Contains(Input input)
    {
        return input is PhysicalInput deviceInput && deviceInputs.Contains(deviceInput);
    }

    #endregion
}
