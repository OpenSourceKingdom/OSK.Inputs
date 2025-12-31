namespace OSK.Inputs.Abstractions.Events;

public class InputUserJoinedEvent(IInputUser user): InputUserEvent(user.Id)
{
    public IInputUser User => user;
}
