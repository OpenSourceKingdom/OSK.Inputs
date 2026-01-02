using System.Collections.Generic;

namespace OSK.Inputs.Abstractions.Configuration;

public class CustomInputScheme
{
    #region Variables

    public required string DefinitionName { get; init; }

    public required string Name { get; init; }

    public required List<InputDeviceMap> DeviceMaps { get; init; }

    #endregion

    #region Helpers

    public InputScheme ToInputScheme()
        => new InputScheme(Name, DeviceMaps, false, true);

    #endregion
}
