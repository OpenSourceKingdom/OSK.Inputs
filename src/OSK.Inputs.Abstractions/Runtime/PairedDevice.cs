namespace OSK.Inputs.Abstractions.Runtime;

public class PairedDevice(int userId, RuntimeDeviceIdentifier deviceIdentifier)
{
    #region Variables

    public int UserId => userId;

    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;

    public DeviceStatus Status { get; private set; }

    #endregion

    #region Helpers

    internal void UpdateStatus(DeviceStatus status) 
    {
        Status = status;
    }

    #endregion
}
