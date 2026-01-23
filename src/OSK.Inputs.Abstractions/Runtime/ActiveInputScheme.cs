using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// The active scheme that a user is using to interact with the input system.
/// See <see cref="InputScheme"/>
/// </summary>
/// <param name="definitionName">The name of the <see cref="InputDefinition"/></param>
/// <param name="schemeName">The name of the <see cref="InputScheme"/></param>
/// <param name="deviceFamilies">The list of devices associated with the active scheme</param>
public readonly struct ActiveInputScheme(string definitionName, string schemeName, InputDeviceFamily[] deviceFamilies)
{
    #region Variables

    public string DefinitionName => definitionName;

    public string SchemeName => schemeName;

    public string DeviceCombinationId { get; } = InputDeviceCombination.GetCombinationId(deviceFamilies);

    public InputDeviceFamily[] DeviceFamilies => deviceFamilies;

    #endregion
}
