namespace OSK.Inputs.Models.Runtime;
public class UserActivatedInput(int userId, ActivatedInput input)
{
    public ActivatedInput ActivatedInput => input;

    public int UserId => userId;
}
