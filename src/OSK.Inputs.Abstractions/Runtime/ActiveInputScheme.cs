using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// The active scheme that a user is using to interact with the input system.
/// See <see cref="InputScheme"/>
/// </summary>
/// <param name="DefinitionName">The name of the <see cref="InputDefinition"/></param>
/// <param name="SchemeName">The name of the <see cref="InputScheme"/></param>
public readonly record struct ActiveInputScheme(string DefinitionName, string SchemeName)
{
}
