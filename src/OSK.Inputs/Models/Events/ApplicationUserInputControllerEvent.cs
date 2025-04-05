using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Models.Events;
public class ApplicationUserInputControllerEvent(IApplicationInputUser user, InputControllerIdentifier controllerIdentifier)
{
    public IApplicationInputUser User => user;

    public InputControllerIdentifier ControllerIdentifier => controllerIdentifier;
}
