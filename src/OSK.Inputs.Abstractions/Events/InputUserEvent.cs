namespace OSK.Inputs.Abstractions.Events;

public abstract class InputUserEvent(int userId): IInputSystemEvent
{
    public int UserId => userId;
}
