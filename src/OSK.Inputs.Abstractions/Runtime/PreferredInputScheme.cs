namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Represents a user's preferred input scheme, and is used with a <see cref="IInputSchemeRepository"/> for data persistence.
/// </summary>
public readonly struct PreferredInputScheme
{
    public required int UserId { get; init; }

    public required string DefinitionName { get; init; }

    public required string SchemeName { get; init; }
}
