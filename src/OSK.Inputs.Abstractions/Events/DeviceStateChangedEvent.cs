namespace OSK.Inputs.Abstractions.Events;

public class DeviceStateChangedEvent(RuntimeDeviceIdentifier deviceIdentifier, DeviceStatus status): UnrecognizedDeviceEvent(deviceIdentifier)
{
    public DeviceStatus Status => status;
}
