using System.Collections.Generic;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Represents a map between an input scheme and an input definition that can be used to trigger configured actions in
/// a system
/// </summary>
/// <param name="deviceSchemeMaps">The scheme's supported device maps</param>
public class InputSchemeActionMap(IEnumerable<DeviceSchemeActionMap> deviceSchemeMaps)
{
    /// <summary>
    /// The scheme's supported device maps and their associated actions
    /// </summary>
    public IEnumerable<DeviceSchemeActionMap> DeviceSchemeMaps => deviceSchemeMaps;
}
