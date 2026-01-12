using System.Collections.Generic;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Represents a map between an input scheme and an input definition that can be used to trigger configured actions in
/// a system
/// </summary>
/// <param name="definitionName">The definition the action map refers to</param>
/// <param name="schemeName">The scheme the action map refers to</param>
/// <param name="deviceSchemeMaps">The scheme's supported device maps</param>
public class InputSchemeActionMap(string definitionName, string schemeName, IEnumerable<DeviceSchemeActionMap> deviceSchemeMaps)
{
    /// <summary>
    /// The definition the action map refers to
    /// </summary>
    public string DefinitionName => definitionName;

    /// <summary>
    /// The scheme the action map refers to
    /// </summary>
    public string SchemeName => schemeName;

    /// <summary>
    /// The scheme's supported device maps and their associated actions
    /// </summary>
    public IEnumerable<DeviceSchemeActionMap> DeviceSchemeMaps => deviceSchemeMaps;
}
