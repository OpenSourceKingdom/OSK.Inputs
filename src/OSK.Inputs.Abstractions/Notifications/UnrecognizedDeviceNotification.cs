using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class UnrecognizedDeviceNotification(RuntimeDeviceIdentifier device): InputDeviceNotification(device)
{
}
