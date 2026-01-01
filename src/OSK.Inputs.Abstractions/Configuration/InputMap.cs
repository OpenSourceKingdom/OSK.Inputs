namespace OSK.Inputs.Abstractions.Configuration;

public readonly struct InputMap
{
    #region Variables

    public required int InputId { get; init; }

    public required string ActionName { get; init; }

    #endregion
}