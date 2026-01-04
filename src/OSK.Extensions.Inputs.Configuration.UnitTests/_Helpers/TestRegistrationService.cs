using OSK.Extensions.Inputs.Configuration.Attributes;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Extensions.Inputs.Configuration.UnitTests._Helpers;

public class TestRegistrationService
{
    public void ValidMethodA(InputEventContext context)
    {
    }

    [InputAction("SpecialAction", [])]
    public void ValidMethodB(InputEventContext context)
    {
    }

    public ValueTask InvalidMethodA(InputEventContext context)
    {
        return ValueTask.CompletedTask;
    }

    public int InvalidMethodB(InputEventContext context)
    {
        return 1;
    }

    public void InvalidMethodC()
    {
    }

    public void InvalidMethodD(InputEventContext context, int a)
    {
    }
}
