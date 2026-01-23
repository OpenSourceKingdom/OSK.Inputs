using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// A specific map for virtual inputs
/// </summary>
public readonly struct VirtualInputMap
{
    /// <summary>
    /// The virtual input this map references
    /// </summary>
    public VirtualInput Input { get; init; }

    /// <summary>
    /// The definition's action name the input maps to
    /// </summary>
    public string ActionName { get; init; }
}
