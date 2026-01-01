using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public class DevicePairedNotification(int userId, RuntimeDeviceIdentifier deviceIdentifier)
    : InputDeviceNotification(deviceIdentifier)
{
    public int PairedUserId => userId;
}
