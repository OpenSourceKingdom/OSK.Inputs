using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class UserDeviceDisconnectedNotification(int userId, RuntimeDeviceIdentifier deviceIdentifier)
    : UserDeviceNotification(userId, deviceIdentifier)
{
}
