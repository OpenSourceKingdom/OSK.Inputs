using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public class InputConfiguration<TOptions>(IInput input, TOptions options): InputConfiguration(input)
    where TOptions: InputOptions
{
    public TOptions Options => options;
}
