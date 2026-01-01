using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public class UserDeviceDisconnectedNotification(int userId, RuntimeDeviceIdentifier deviceIdentifier)
    : UserDeviceNotification(userId, deviceIdentifier)
{
}
