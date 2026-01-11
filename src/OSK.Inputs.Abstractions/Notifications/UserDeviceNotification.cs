using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public abstract class UserDeviceNotification(IInputUser user, RuntimeDeviceIdentifier deviceIdentifier)
    : InputDeviceNotification(deviceIdentifier)
{
    public IInputUser User => user;
}
