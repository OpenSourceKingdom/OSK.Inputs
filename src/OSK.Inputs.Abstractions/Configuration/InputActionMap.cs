using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// A map between an input and the <see cref="InputAction"/> it is associated to with an <see cref="InputDefinition"/>
/// </summary>
public class InputActionMap
{
    #region Variables

    /// <summary>
    /// The unique id for the input
    /// </summary>
    public int InputId => Input.Id;

    /// <summary>
    /// The specific input this map references
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// The specific action this map references
    /// </summary>
    public required InputAction Action { get; init; }

    /// <summary>
    /// The collection of inputs this input is associated with.
    /// For example, a virtual input will be linked to other inputs
    /// </summary>
    public required int[] LinkedInputIds { get; init; }

    #endregion
}
