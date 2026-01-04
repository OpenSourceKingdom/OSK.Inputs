using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class DeviceStateChangedNotification(RuntimeDeviceIdentifier deviceIdentifier, DeviceStatus status)
    : UnrecognizedDeviceNotification(deviceIdentifier)
{
    public DeviceStatus Status => status;
}
