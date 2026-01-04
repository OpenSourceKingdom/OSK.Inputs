namespace OSK.Inputs.Abstractions.Notifications;

public abstract class InputUserNotification(int userId): IInputNotification
{
    public int UserId => userId;
}
