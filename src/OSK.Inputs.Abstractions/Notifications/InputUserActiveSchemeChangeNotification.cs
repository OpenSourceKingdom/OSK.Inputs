using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions.Notifications;

public class InputUserActiveSchemeChangeNotification(IInputUser user, ActiveInputScheme scheme): InputUserNotification(user)
{
    public ActiveInputScheme NewScheme => scheme;
}
