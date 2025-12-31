namespace OSK.Inputs.Abstractions;

public class InputActivationContext(int userId, InputActivation activation, 
    PointerDetails pointerInformation, InputActivityInformation activityInformation)
{
    public int UserId => userId;

    public InputActivation Activation => activation;

    public PointerDetails PointerInformation => pointerInformation;

    public InputActivityInformation ActivityInformation => activityInformation;
}
