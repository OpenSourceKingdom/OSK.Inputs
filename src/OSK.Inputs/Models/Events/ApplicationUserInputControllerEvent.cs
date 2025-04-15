using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Models.Events;
public class ApplicationUserInputControllerEvent(IApplicationInputUser user, InputDeviceIdentifier controllerIdentifier)
{
    public IApplicationInputUser User => user;

    public InputDeviceIdentifier ControllerIdentifier => controllerIdentifier;
}
