using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Abstractions.Notifications;

public class InputProcessingStateChangedNotification(bool processingInput): InputSystemNotification
{
    public bool ProcessingInput => processingInput;
}
