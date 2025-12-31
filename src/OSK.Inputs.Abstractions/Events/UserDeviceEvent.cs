namespace OSK.Inputs.Abstractions.Events;

public class UserDeviceEvent(int userId, RuntimeDeviceIdentifier deviceIdentifier, DeviceEventType eventType)
    : InputDeviceEvent(deviceIdentifier)
{
    public int UserId => userId;

    public DeviceEventType EventType => eventType;
}
