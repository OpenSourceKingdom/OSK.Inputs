using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public class InputUserRemovedNotification(int userId): InputUserNotification(userId)
{
}
