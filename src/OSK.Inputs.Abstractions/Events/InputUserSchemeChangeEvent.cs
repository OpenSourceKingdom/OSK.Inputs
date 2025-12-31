namespace OSK.Inputs.Abstractions.Events;

public class InputUserSchemeChangeEvent(int userId, ActiveInputScheme scheme): InputUserEvent(userId)
{
    public ActiveInputScheme NewScheme => scheme;
}
