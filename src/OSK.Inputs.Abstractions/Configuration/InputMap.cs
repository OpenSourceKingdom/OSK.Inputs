using System.Diagnostics.CodeAnalysis;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Defines a map between an <see cref="Input"/> and one of the configured <see cref="InputDefinition.Actions"/>
/// </summary>
public readonly struct InputMap
{
    #region Variables

    private readonly bool _isPassive;
    private readonly string? _actionName;

    #endregion

    #region Api

    /// <summary>
    /// The unique id for the input on the device
    /// </summary>
    public required int InputId { get; init; }

    /// <summary>
    /// The definition's action name the input maps to
    /// </summary>
    public string? ActionName
    {
        get => _actionName;
        init
        {
            _actionName = value;
            _isPassive = _actionName is null;
        }
    }

    /// <summary>
    /// Indicates if the given input triggers an action in the system or if it provides data passively
    /// </summary>
    [MemberNotNullWhen(false, nameof(ActionName))]
    public bool IsPassive => _isPassive;

    #endregion
}