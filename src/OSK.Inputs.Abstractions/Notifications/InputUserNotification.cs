namespace OSK.Inputs.Abstractions.Notifications;

public abstract class InputUserNotification(IInputUser user): IInputNotification
{
    public IInputUser User => user;
}
