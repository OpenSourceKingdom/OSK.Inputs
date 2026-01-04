using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class UserDeviceConnectedNotification(int userId, RuntimeDeviceIdentifier deviceIdentifier)
    : UserDeviceNotification(userId, deviceIdentifier)
{
}
