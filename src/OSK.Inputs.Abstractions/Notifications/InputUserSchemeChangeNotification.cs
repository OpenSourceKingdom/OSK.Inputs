using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class InputUserSchemeChangeNotification(int userId, ActiveInputScheme scheme): InputUserNotification(userId)
{
    public ActiveInputScheme NewScheme => scheme;
}
