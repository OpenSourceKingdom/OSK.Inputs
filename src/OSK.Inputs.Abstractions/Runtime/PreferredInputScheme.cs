namespace OSK.Inputs.Abstractions.Runtime;

public readonly struct PreferredInputScheme
{
    public required int UserId { get; init; }

    public required string DefinitionName { get; init; }

    public required string SchemeName { get; init; }
}
