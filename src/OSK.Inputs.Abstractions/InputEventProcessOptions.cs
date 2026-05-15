using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions;

public class InputEventProcessOptions
{
    #region Variables

    private IReadOnlyCollection<int> _suppressedActionTypes = [];

    public IReadOnlyCollection<int> SuppressedActionGroups
    {
        get => _suppressedActionTypes;
        set => _suppressedActionTypes = [.. value.Distinct()];
    }

    #endregion
}
