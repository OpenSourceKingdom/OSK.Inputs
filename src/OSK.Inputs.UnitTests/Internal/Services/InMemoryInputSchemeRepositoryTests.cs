using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;
public class InMemoryInputSchemeRepositoryTests
{
    #region Variables

    private readonly MockOutputFactory _mockOutputFactory;

    private readonly InMemoryInputSchemeRepository _repository;

    #endregion

    #region Constructors

    public InMemoryInputSchemeRepositoryTests()
    {
        _mockOutputFactory = new MockOutputFactory();

        _repository = new(_mockOutputFactory);
    }

    #endregion

    #region SaveCustomInputSchemeAsync

    [Fact]
    public async Task SaveCustomInputSchemeAsync_NullInputScheme_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveCustomInputSchemeAsync("abc", null!));
    }

    [Fact]
    public async Task SaveCustomInputSchemeAsync_NewInputScheme_AddsToInternalData()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "abc", "abc", false, []);

        // Act
        var result = await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);

        // Assert
        Assert.True(result.IsSuccessful);

        Assert.Single(_repository._customInputSchemes);
        Assert.True(_repository._customInputSchemes.TryGetValue(inputScheme.InputDefinitionName, out var controllerSchemeLookup));
        Assert.True(controllerSchemeLookup.TryGetValue(inputScheme.ControllerName.Name, out var schemeLookup));
        Assert.True(schemeLookup.TryGetValue(inputScheme.SchemeName, out var scheme));
        Assert.Equal(inputScheme, scheme);
    }

    [Fact]
    public async Task SaveCustomInputSchemeAsync_InputSchemeMatchesOriginal_OverwritesInternalData()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "abc", "abc", false, []);
        var inputScheme2 = new InputScheme("abc", "abc", "abc", false, []);

        await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);

        // Act
        var result = await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme2);

        // Assert
        Assert.True(result.IsSuccessful);

        Assert.Single(_repository._customInputSchemes);
        Assert.True(_repository._customInputSchemes.TryGetValue(inputScheme.InputDefinitionName, out var controllerSchemeLookup));
        Assert.True(controllerSchemeLookup.TryGetValue(inputScheme.ControllerName.Name, out var schemeLookup));
        Assert.True(schemeLookup.TryGetValue(inputScheme.SchemeName, out var scheme));
        Assert.Equal(inputScheme2, scheme);
    }

    #endregion

    #region GetCustomInputSchemesAsync

    [Fact]
    public async Task GetCustomInputSchemesAsync_NoInputSchemesForDefinition_ReturnsEmptyList()
    {
        // Arrange
        List<InputScheme> inputSchemes = [new InputScheme("abc", "abc", "abc", false, [])];

        foreach (var inputScheme in inputSchemes)
        {
            await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);
        }

        // Act
        var result = await _repository.GetCustomInputSchemesAsync("def");

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetCustomInputSchemesAsync_DefinitionHasInputSchemes_ReturnsExpectedList()
    {
        // Arrange
        List<InputScheme> inputSchemes = [new InputScheme("abc", "abc", "abc", false, []), new InputScheme("abc", "abc", "def", false, [])];

        foreach (var inputScheme in inputSchemes)
        {
            await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);
        }

        // Act
        var result = await _repository.GetCustomInputSchemesAsync("abc");

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotEmpty(result.Value);
        Assert.True(inputSchemes.All(scheme => result.Value.Contains(scheme)));
    }

    #endregion

    #region GetCustomInputSchemeAsync

    [Fact]
    public async Task GetCustomInputSchemeAsync_DefinitionNameNotInDataSet_ReturnsNotFound()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "abc", "abc", false, []);

        await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);

        // Act
        var result = await _repository.GetCustomInputSchemeAsync("def", new InputControllerName("abc"), "abc");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task GetCustomInputSchemeAsync_InputControllerNameNotInDataSet_ReturnsNotFound()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "abc", "abc", false, []);

        await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);

        // Act
        var result = await _repository.GetCustomInputSchemeAsync("abc", new InputControllerName("def"), "abc");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task GetCustomInputSchemeAsync_InputSchemeNameNotInDataSet_ReturnsNotFound()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "abc", "abc", false, []);

        await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);

        // Act
        var result = await _repository.GetCustomInputSchemeAsync("abc", new InputControllerName("abc"), "def");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task GetCustomInputSchemeAsync_Valid_ReturnsExpectedInputScheme()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "abc", "abc", false, []);

        await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);

        // Act
        var result = await _repository.GetCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme.ControllerName, inputScheme.SchemeName);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(inputScheme, result.Value);
    }

    #endregion

    #region DeleteCustomInputSchemeAsync

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputDefinitionNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange/Act
        var result = await _repository.DeleteCustomInputSchemeAsync("abc", new InputControllerName("abc"), "abc");

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputControllerNamenNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "def", "hij", false, []);

        // Act
        var result = await _repository.DeleteCustomInputSchemeAsync("abc", new InputControllerName("abc"), "abc");

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputSchemeNamenNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "def", "hij", false, []);

        // Act
        var result = await _repository.DeleteCustomInputSchemeAsync("abc", new InputControllerName("def"), "abc");

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_ValidInputScheme_ReturnsSuccessfully()
    {
        // Arrange
        var inputScheme = new InputScheme("abc", "def", "hij", false, []);

        await _repository.SaveCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme);

        // Act
        var result = await _repository.DeleteCustomInputSchemeAsync(inputScheme.InputDefinitionName, inputScheme.ControllerName, inputScheme.SchemeName);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Empty(_repository._customInputSchemes.SelectMany(schemeControllerGroup => schemeControllerGroup.Value.Values.SelectMany(schemeGroup => schemeGroup.Values.Select(kvp => kvp))));
    }

    #endregion

    #region SaveActiveInputSchemeAsync

    [Fact]
    public async Task SaveActiveInputSchemeAsync_NullActiveScheme_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveActiveInputSchemeAsync(null!));
    }

    [Fact]
    public async Task SaveActiveInputSchemeAsync_NoActiveInputScheme_AddsToInternalData()
    {
        // Arrange
        var activeScheme = new ActiveInputScheme(1, "abc", "abc", "abc");

        // Act
        var result = await _repository.SaveActiveInputSchemeAsync(activeScheme);

        // Assert
        Assert.True(result.IsSuccessful);

        Assert.Single(_repository._activeSchemes);
        Assert.True(_repository._activeSchemes.TryGetValue(activeScheme.UserId, out var userInputDefinitionSchemeLookup));
        Assert.True(userInputDefinitionSchemeLookup.TryGetValue(activeScheme.InputDefinitionName, out var controllerSchemeLookup));
        Assert.True(controllerSchemeLookup.TryGetValue(activeScheme.ActiveInputSchemeName, out var userActiveScheme));

        Assert.Equal(activeScheme, userActiveScheme);
    }

    [Fact]
    public async Task SaveActiveInputSchemeAsync_ActiveInputSchemeInDataSet_OverwritesInternalData()
    {
        // Arrange
        var activeScheme = new ActiveInputScheme(1, "abc", "abc", "abc");
        var activeScheme2 = new ActiveInputScheme(1, "abc", "abc", "def");

        await _repository.SaveActiveInputSchemeAsync(activeScheme);

        // Act
        var result = await _repository.SaveActiveInputSchemeAsync(activeScheme2);

        // Assert
        Assert.True(result.IsSuccessful);

        Assert.Single(_repository._activeSchemes);
        Assert.True(_repository._activeSchemes.TryGetValue(activeScheme.UserId, out var userInputDefinitionSchemeLookup));
        Assert.True(userInputDefinitionSchemeLookup.TryGetValue(activeScheme.InputDefinitionName, out var controllerSchemeLookup));
        Assert.True(controllerSchemeLookup.TryGetValue(activeScheme.ActiveInputSchemeName, out var userActiveScheme));

        Assert.Equal(activeScheme2, userActiveScheme);
    }

    #endregion

    #region GetActiveInputSchemesAsync

    [Fact]
    public async Task GetActiveInputSchemesAsync_NoActiveInputsSchmes_ReturnsEmptyList()
    {
        // Arrange/Act
        var result = await _repository.GetActiveInputSchemesAsync(1, "abc");

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetActiveInputSchemesAsync_MultipleUsers_MultipleDefinitionsHaveActiveInputSchemes_MultipleControllersHaveActiveInputsSchmes_ReturnsListForSpecifiedDefinition()
    {
        // Arrange
        List<ActiveInputScheme> activeSchemes = [
            new ActiveInputScheme(1, "abc", "abc", "abc"),
            new ActiveInputScheme(1, "abc", "def", "abc"),
            new ActiveInputScheme(1, "def", "def", "abc"),

            new ActiveInputScheme(2, "abc", "abc", "abc"),
            new ActiveInputScheme(3, "abc", "abc", "abc"),
        ];

        foreach (var activeScheme in activeSchemes)
        {
            await _repository.SaveActiveInputSchemeAsync(activeScheme);
        }

        // Act
        var result = await _repository.GetActiveInputSchemesAsync(1, "abc");

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(2, result.Value.Count());

        Assert.Contains(activeSchemes[0], result.Value);
        Assert.Contains(activeSchemes[1], result.Value);
    }

    #endregion

    #region DeleteActiveInputSchemeAsync

    [Fact]
    public async Task DeleteActiveInputSchemeAsync_UserIdNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange/Act
        var result = await _repository.DeleteActiveInputSchemeAsync(1, "abc", new InputControllerName("abc"));

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task DeleteActiveInputSchemeAsync_InputDefinitionNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange
        await _repository.SaveActiveInputSchemeAsync(new ActiveInputScheme(1, "abc", "abc", "abc"));

        // Act
        var result = await _repository.DeleteActiveInputSchemeAsync(1, "def", new InputControllerName("abc"));

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Single(_repository._activeSchemes);
    }

    [Fact]
    public async Task DeleteActiveInputSchemeAsync_ControllerNameNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange
        await _repository.SaveActiveInputSchemeAsync(new ActiveInputScheme(1, "abc", "abc", "abc"));

        // Act
        var result = await _repository.DeleteActiveInputSchemeAsync(1, "abc", new InputControllerName("def"));

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Single(_repository._activeSchemes);
    }

    [Fact]
    public async Task DeleteActiveInputSchemeAsync_Valid_RemovesActiveInputScheme_ReturnsSuccessfully()
    {
        // Arrange
        await _repository.SaveActiveInputSchemeAsync(new ActiveInputScheme(1, "abc", "abc", "abc"));

        // Act
        var result = await _repository.DeleteActiveInputSchemeAsync(1, "abc", new InputControllerName("abc"));

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Empty(_repository._activeSchemes.SelectMany(schemeControllerGroup => schemeControllerGroup.Value.Values.SelectMany(schemeGroup => schemeGroup.Values.Select(kvp => kvp))));
    }

    #endregion
}
