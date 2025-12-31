using System.Collections.Generic;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions;

public class CustomInputScheme
{
    public required string DefinitionName { get; init; }

    public required string Name { get; init; }

    public required List<InputDeviceMap> DeviceMaps { get; init; }
}
