using Moq;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
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
        var repository = new InMemorySchemeRepository(Mock.Of<IOutputFactory<InMemorySchemeRepository>>());

        // Act/Assert
        Assert.False(repository.AllowCustomSchemes);
    }

    #endregion

    #region SavePreferredSchemeAsync

    [Fact]
    public async Task SavePreferredSchemeAsync_NewScheme_AddsToStorage_ReturnsSuccessfully()
    {
        // Arrange
        var repository = new InMemorySchemeRepository(new MockOutputFactory<InMemorySchemeRepository>());
        var scheme = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Abc" };

        // Act
        var schemeOutput = await repository.SavePreferredSchemeAsync(scheme);

        // Assert
        Assert.True(schemeOutput.IsSuccessful);

        Assert.Single(repository._preferredSchemeLookup);
        Assert.Single(repository._preferredSchemeLookup[1]);
    }

    [Fact]
    public async Task SavePreferredSchemeAsync_ExistingScheme_OverwritesStorage_ReturnsSuccessfully()
    {
        // Arrange
        var repository = new InMemorySchemeRepository(new MockOutputFactory<InMemorySchemeRepository>());
        var scheme1 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Abc" };
        var scheme2 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Def" };

        // Act
        var schemesOutput1 = await repository.SavePreferredSchemeAsync(scheme1);
        var schemesOutput2 = await repository.SavePreferredSchemeAsync(scheme2);

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
        var repository = new InMemorySchemeRepository(new MockOutputFactory<InMemorySchemeRepository>());
        var scheme1 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Abc" };
        var scheme2 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Def" };
        var scheme3 = new PreferredInputScheme() { UserId = 1, DefinitionName = "Def", SchemeName = "Abc" };

        // Act
        var schemesOutput1 = await repository.SavePreferredSchemeAsync(scheme1);
        var schemesOutput2 = await repository.SavePreferredSchemeAsync(scheme3);
        var schemesOutput3 = await repository.SavePreferredSchemeAsync(scheme2);

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
        var repository = new InMemorySchemeRepository(new MockOutputFactory<InMemorySchemeRepository>());

        repository._preferredSchemeLookup[1] = [
            new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Abc"}
        ];
        repository._preferredSchemeLookup[2] = [
            new PreferredInputScheme() { UserId = 2, DefinitionName = "Abc", SchemeName = "Abc"},
            new PreferredInputScheme() { UserId = 2, DefinitionName = "Def", SchemeName = "Abc"}
        ];
        repository._preferredSchemeLookup[3] = [
            new PreferredInputScheme() { UserId = 3, DefinitionName = "Abc", SchemeName = "Abc"}
        ];

        // Act
        var schemesOutput = await repository.GetPreferredSchemesAsync();

        // Assert
        Assert.True(schemesOutput.IsSuccessful);
        Assert.Equal(4, schemesOutput.Value.Count());
    }

    #endregion
}
