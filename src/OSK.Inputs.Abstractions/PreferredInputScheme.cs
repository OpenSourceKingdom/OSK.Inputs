namespace OSK.Inputs.Abstractions;

public readonly struct PreferredInputScheme
{
    public required int UserId { get; init; }

    public required string DefinitionName { get; init; }

    public required string SchemeName { get; init; }
}
