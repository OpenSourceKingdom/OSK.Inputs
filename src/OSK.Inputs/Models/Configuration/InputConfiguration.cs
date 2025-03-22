using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public class InputConfiguration(IInput input)
{
    public IInput Input => input;
}
