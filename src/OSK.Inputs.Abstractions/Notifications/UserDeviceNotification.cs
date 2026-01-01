using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public class UserDeviceNotification(int userId, RuntimeDeviceIdentifier deviceIdentifier)
    : InputDeviceNotification(deviceIdentifier)
{
    public int UserId => userId;
}
