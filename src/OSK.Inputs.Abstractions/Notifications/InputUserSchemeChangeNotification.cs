using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions.Notifications;

public class InputUserSchemeChangeNotification(int userId, ActiveInputScheme scheme): InputUserNotification(userId)
{
    public ActiveInputScheme NewScheme => scheme;
}
