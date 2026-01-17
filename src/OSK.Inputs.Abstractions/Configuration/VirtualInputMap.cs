using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

public readonly struct VirtualInputMap
{
    public VirtualInput Input { get; init; }

    /// <summary>
    /// The definition's action name the input maps to
    /// </summary>
    public string ActionName { get; init; }
}
