using OSK.Inputs.Attributes;
using OSK.Inputs.Models.Events;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestRegistrationService
{
    public ValueTask ValidMethodA(InputActivationEvent activationEvent)
    {
        return ValueTask.CompletedTask;
    }

    [InputAction("SpecialAction")]
    public ValueTask ValidMethodB(InputActivationEvent activationEvent)
    {
        return ValueTask.CompletedTask;
    }

    public void InvalidMethodA(InputActivationEvent activationEvent) 
    { 
    }

    public int InvalidMethodB(InputActivationEvent activationEvent)
    {
        return 1;
    }

    public ValueTask InvalidMethodC()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask InvalidMethodD(InputActivationEvent activationEvent, int a)
    {
        return ValueTask.CompletedTask;
    }
}
