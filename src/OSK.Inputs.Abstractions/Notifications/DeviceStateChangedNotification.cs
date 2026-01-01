using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public class DeviceStateChangedNotification(RuntimeDeviceIdentifier deviceIdentifier, DeviceStatus status)
    : UnrecognizedDeviceNotification(deviceIdentifier)
{
    public DeviceStatus Status => status;
}
