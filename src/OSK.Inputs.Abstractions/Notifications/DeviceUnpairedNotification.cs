using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class DeviceUnpairedNotification(int userId, RuntimeDeviceIdentifier deviceIdentifier)
    : InputDeviceNotification(deviceIdentifier)
{
    public int UnpairedUserId => userId;
}
