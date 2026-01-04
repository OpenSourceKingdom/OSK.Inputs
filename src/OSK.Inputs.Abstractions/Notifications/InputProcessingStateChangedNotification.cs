namespace OSK.Inputs.Abstractions.Notifications;

public class InputProcessingStateChangedNotification(bool processingInput): InputSystemNotification
{
    public bool ProcessingInput => processingInput;
}
