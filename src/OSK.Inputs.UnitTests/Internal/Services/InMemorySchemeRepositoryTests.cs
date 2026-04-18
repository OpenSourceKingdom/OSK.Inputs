using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Services;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InMemorySchemeRepositoryTests
{
    #region AllowCustomSchemes

    [Fact]
    public void AllowCustomSchemes_ReturnsFalse()
    {
        // Arrange
        var repository = new InMemorySchemeRepository();

        // Act/Assert
        Assert.False(repository.AllowCustomSchemes);
    }

    #endregion

    #region SavePreferredSchemeAsync

    [Fact]
    public async Task SavePreferredSchemeAsync_NewScheme_AddsToStorage_ReturnsSuccessfully()
    {
        // Arrange
        var repository = new InMemorySchemeRepository();
        var scheme = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", CombinationId = "Abc", SchemeName = "Abc" };

        // Act
        var schemeOutput = await repository.SavePreferredSchemeAsync(scheme, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(schemeOutput.IsSuccessful);

        Assert.Single(repository._preferredSchemeLookup);
        Assert.Single(repository._preferredSchemeLookup[1]);
    }

    [Fact]
    public async Task SavePreferredSchemeAsync_ExistingScheme_OverwritesStorage_ReturnsSuccessfully()
    {
        // Arrange
        var repository = new InMemorySchemeRepository();
        var scheme1 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", CombinationId = "Abc", SchemeName = "Abc" };
        var scheme2 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", CombinationId = "Abc", SchemeName = "Def" };

        // Act
        var schemesOutput1 = await repository.SavePreferredSchemeAsync(scheme1, TestContext.Current.CancellationToken);
        var schemesOutput2 = await repository.SavePreferredSchemeAsync(scheme2, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(schemesOutput1.IsSuccessful);
        Assert.True(schemesOutput2.IsSuccessful);

        Assert.Single(repository._preferredSchemeLookup);
        Assert.Single(repository._preferredSchemeLookup[1]);
        Assert.Equal(scheme2.SchemeName, repository._preferredSchemeLookup[1].Single().SchemeName);
    }

    [Fact]
    public async Task SavePreferredSchemeAsync_ExistingAndNewSchemes_ReturnsSuccessfully()
    {
        // Arrange
        var repository = new InMemorySchemeRepository();
        var scheme1 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", CombinationId = "Abc", SchemeName = "Abc" };
        var scheme2 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", CombinationId = "Def", SchemeName = "Def" };
        var scheme3 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Def", CombinationId = "Ghi", SchemeName = "Abc" };

        // Act
        var schemesOutput1 = await repository.SavePreferredSchemeAsync(scheme1, TestContext.Current.CancellationToken);
        var schemesOutput2 = await repository.SavePreferredSchemeAsync(scheme3, TestContext.Current.CancellationToken);
        var schemesOutput3 = await repository.SavePreferredSchemeAsync(scheme2, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(schemesOutput1.IsSuccessful);
        Assert.True(schemesOutput2.IsSuccessful);
        Assert.True(schemesOutput3.IsSuccessful);

        Assert.Single(repository._preferredSchemeLookup);
        Assert.Equal(2, repository._preferredSchemeLookup[1].Count);
        Assert.Single(repository._preferredSchemeLookup[1], s => s.SchemeName == scheme3.SchemeName);
        Assert.Single(repository._preferredSchemeLookup[1], s => s.SchemeName == scheme2.SchemeName);
    }

    #endregion

    #region GetPreferredSchemesAsync

    [Fact]
    public async Task GetPreferredSchemesAsync_ReturnsAllStoredSchemes()
    {
        // Arrange
        var repository = new InMemorySchemeRepository();

        repository._preferredSchemeLookup[1] = [
            new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", CombinationId = "Abc", SchemeName = "Abc"}
        ];
        repository._preferredSchemeLookup[2] = [
            new PreferredInputScheme() { UserId = 2, DefinitionName = "Abc", CombinationId = "Abc", SchemeName = "Abc"},
            new PreferredInputScheme() { UserId = 2, DefinitionName = "Def", CombinationId = "Abc", SchemeName = "Abc"}
        ];
        repository._preferredSchemeLookup[3] = [
            new PreferredInputScheme() { UserId = 3, DefinitionName = "Abc", CombinationId = "Abc", SchemeName = "Abc"}
        ];

        // Act
        var schemesOutput = await repository.GetPreferredSchemesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(schemesOutput.IsSuccessful);
        Assert.Equal(4, schemesOutput.Data.Count());
    }

    #endregion
}
