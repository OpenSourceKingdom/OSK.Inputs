namespace OSK.Inputs.Abstractions.Notifications;

public class InputUserJoinedNotification(IInputUser user): InputUserNotification(user.Id)
{
    public IInputUser User => user;
}
