namespace OSK.Inputs.Models.Runtime;
public class UserActivatedInput(int userId, ActivatedInput input)
    : ActivatedInput(input.ControllerIdentifier, input.Input, input.ActionKey, input.TriggeredPhase, input.InputPower, input.PointerTranslation)
{
    public int UserId => userId;
}
