using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class UserDeviceDisconnectedNotification(IInputUser user, RuntimeDeviceIdentifier deviceIdentifier)
    : UserDeviceNotification(user, deviceIdentifier)
{
}
