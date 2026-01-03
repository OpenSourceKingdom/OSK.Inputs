using System;

namespace OSK.Inputs.Abstractions.Runtime;

public class InputEventContext(int userId, InputEvent inputEvent, PointerDetails pointerDetails, 
    InputActivityInformation activityInformation, IServiceProvider serviceProvider)
{
    public int UserId => userId;

    public InputEvent Event => inputEvent;

    public PointerDetails PointerDetails => pointerDetails;

    public InputActivityInformation ActivityInformation => activityInformation;

    public IServiceProvider Services => serviceProvider;
}
