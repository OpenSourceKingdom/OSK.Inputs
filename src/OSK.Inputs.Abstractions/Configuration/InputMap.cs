using System.Collections.Generic;

namespace OSK.Inputs.Abstractions.Configuration;

public readonly struct InputMap
{
    #region Variables

    public int InputId { get; init; }

    public string ActionName { get; init; }

    #endregion
}