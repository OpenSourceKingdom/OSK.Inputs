using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class DevicePairedNotification(int userId, RuntimeDeviceIdentifier deviceIdentifier)
    : InputDeviceNotification(deviceIdentifier)
{
    public int PairedUserId => userId;
}
