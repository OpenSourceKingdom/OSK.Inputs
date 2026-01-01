using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public class UnrecognizedDeviceNotification(RuntimeDeviceIdentifier device): InputDeviceNotification(device)
{
}
