using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Abstractions.Notifications;

public class InputUserActiveDefinitionChangeNotification(IInputUser user, string definitionName): InputUserNotification(user)
{
    public string ActiveDefinitionName => definitionName;
}
