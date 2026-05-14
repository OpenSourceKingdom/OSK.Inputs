using System.Diagnostics.CodeAnalysis;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions;

public class ProcessedInputResult
{
    [MemberNotNullWhen(true, nameof(MatchedAction))]
    public bool Triggered => MatchedAction is not null;

    public InputActionMap? MatchedAction { get; set; }

    public bool ConsumedInput { get; set; }
}
