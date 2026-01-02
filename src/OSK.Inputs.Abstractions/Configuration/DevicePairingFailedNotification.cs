using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Configuration;

public class DevicePairingFailedNotification(int pairingUserId, RuntimeDeviceIdentifier deviceIdentifier)
    : InputDeviceNotification(deviceIdentifier)
{
    public int AttemptedPairingUserId => pairingUserId;
}
