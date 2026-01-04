using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Defines a map between an <see cref="Input"/> and one of the configured <see cref="InputDefinition.Actions"/>
/// </summary>
public readonly struct InputMap
{
    #region Variables

    /// <summary>
    /// The unique id for the input on the device
    /// </summary>
    public required int InputId { get; init; }

    /// <summary>
    /// The definition's action name the input maps to
    /// </summary>
    public required string ActionName { get; init; }

    #endregion
}