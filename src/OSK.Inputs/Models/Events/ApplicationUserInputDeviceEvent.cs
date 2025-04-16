using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Models.Events;
public class ApplicationUserInputDeviceEvent(IApplicationInputUser user, InputDeviceIdentifier deviceIdentifier)
{
    public IApplicationInputUser User => user;

    public InputDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
