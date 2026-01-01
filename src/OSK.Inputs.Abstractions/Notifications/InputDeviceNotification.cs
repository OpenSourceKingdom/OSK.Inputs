using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public abstract class InputDeviceNotification(RuntimeDeviceIdentifier deviceIdentifier): IInputSystemNotification
{
    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
