using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Options;

public class InputActionOptions
{
    public int? ActionGroup { get; set; }

    public InputStreamType[] IncludedInputStreams { get; set; } = [];

    public string? Description { get; set; }
}
