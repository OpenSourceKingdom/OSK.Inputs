using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public abstract class InputDeviceNotification(RuntimeDeviceIdentifier deviceIdentifier): IInputNotification
{
    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
