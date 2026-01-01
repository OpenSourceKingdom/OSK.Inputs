using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public abstract class InputUserNotification(int userId): IInputSystemNotification
{
    public int UserId => userId;
}
