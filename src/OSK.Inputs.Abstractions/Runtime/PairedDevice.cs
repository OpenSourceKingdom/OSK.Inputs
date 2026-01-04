namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// A paired device is the combination of a <see cref="RuntimeDeviceIdentifier"/> and the user it belongs to.
/// This provides a unique pairing in the input system that can then be used to communicate to the device or
/// process inputs for a user
/// </summary>
/// <param name="userId">The user the device is associated to</param>
/// <param name="deviceIdentifier">The device identifier</param>
public class PairedDevice(int userId, RuntimeDeviceIdentifier deviceIdentifier)
{
    #region Variables

    /// <summary>
    /// The specific id of the user in the input system
    /// </summary>
    public int UserId => userId;

    /// <summary>
    /// The actual device being used to provide interactions into the input system
    /// </summary>
    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;

    /// <summary>
    /// The current status of the device as the input system understands it
    /// </summary>
    public DeviceStatus Status { get; private set; }

    #endregion

    #region Helpers

    internal void UpdateStatus(DeviceStatus status) 
    {
        Status = status;
    }

    #endregion
}
