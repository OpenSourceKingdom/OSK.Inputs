using System.Linq;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// A virtual input that uses a collection of <see cref="IDeviceInput"/>s
/// </summary>
/// <param name="deviceType">The owner device type</param>
/// <param name="id">A unique id for the combination</param>
/// <param name="deviceInputs">The inputs this will use</param>
public class DeviceCombinationInput(InputDeviceType deviceType, int id, params IDeviceInput[] deviceInputs)
    : VirtualInput(deviceType, id)
{
    #region Variables

    /// <summary>
    /// The collection of <see cref="IDeviceInput"/>s this references to be triggered
    /// </summary>
    public IDeviceInput[] DeviceInputs => deviceInputs;

    #endregion

    #region VirtualInputs Overrides

    /// <inheritdoc/>
    public override bool Contains(IInput input)
    {
        return input is IDeviceInput deviceInput && deviceInputs.Contains(deviceInput);
    }

    #endregion
}
