using System.Diagnostics.CodeAnalysis;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// A map between an input and the <see cref="InputAction"/> it is associated to with an <see cref="InputDefinition"/>
/// </summary>
public class InputActionMap
{
    #region Variables

    /// <summary>
    /// The specific input this map references
    /// </summary>
    public required IInput Input { get; init; }

    /// <summary>
    /// The specific action this map references
    /// </summary>
    public required InputAction Action { get; init; }

    #endregion
}
