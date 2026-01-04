using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.UnitTests.Configuration;

public class InputDefinitionTests
{
    #region GetAction

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetAction_InvalidActionName_ReturnsNull(string? actionName)
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        var action = definition.GetAction(actionName!);

        // Assert
        Assert.Null(action);
    }

    [Fact]
    public void GetAction_ActionNameDoesNotExist_ReturnsNull()
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        var action = definition.GetAction("Hello");

        // Assert
        Assert.Null(action);
    }

    [Fact]
    public void GetAction_ValidActionName_ReturnsAction()
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [new InputAction("Hello", new HashSet<InputPhase>(), _ => { })], [], false);

        // Act
        var action = definition.GetAction("Hello");

        // Assert
        Assert.NotNull(action);
    }

    #endregion

    #region GetScheme

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetScheme_InvalidSchemeName_ReturnsNull(string? schemeName)
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        var action = definition.GetScheme(schemeName!);

        // Assert
        Assert.Null(action);
    }

    [Fact]
    public void GetScheme_SchemeNameDoesNotExist_ReturnsNull()
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        var action = definition.GetScheme("Hello");

        // Assert
        Assert.Null(action);
    }

    [Fact]
    public void GetScheme_ValidSchemeName_ReturnsAction()
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [new InputScheme("Hello", [], false, false)], false);

        // Act
        var action = definition.GetScheme("Hello");

        // Assert
        Assert.NotNull(action);
    }

    #endregion

    #region ApplyCustomScheme

    [Fact]
    public void ApplyCustomScheme_NullScheme_ReturnsSuccessfully()
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        definition.ApplyCustomScheme(null!);

        // Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplyCustomScheme_InvalidDefinitionName_DoesNotAddToDefinition_ReturnsSuccessfully(string? definitionName)
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        definition.ApplyCustomScheme(new CustomInputScheme()
        {
            DefinitionName = definitionName!,
            Name = "Hello",
            DeviceMaps = []
        });

        var scheme = definition.GetScheme("Hello");

        // Assert
        Assert.Null(scheme);
    }

    [Fact]
    public void ApplyCustomScheme_DefinitionNameDoesNotMatchDefinition_DoesNotAddToDefinition_ReturnsSuccessfully()
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        definition.ApplyCustomScheme(new CustomInputScheme()
        {
            DefinitionName = "Bye",
            Name = "Hello",
            DeviceMaps = []
        });

        var scheme = definition.GetScheme("Hello");

        // Assert
        Assert.Null(scheme);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplyCustomScheme_InvalidSchemeName_DoesNotAddToDefinition_ReturnsSuccessfully(string? schemeName)
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);
        
        // Act
        definition.ApplyCustomScheme(new CustomInputScheme()
        {
            DefinitionName = "Hello",
            Name = schemeName!,
            DeviceMaps = []
        });

        var scheme = definition.GetScheme(schemeName!);

        // Assert
        Assert.Null(scheme);
    }

    [Fact]
    public void ApplyCustomScheme_ValidSchemeButBuiltInSchemeWithSameNameExists_DoesNotAddToDefinition_ReturnsSuccessfully()
    {
        // Arrange
        var builtInScheme = new InputScheme("Hello", [], false, false);
        var definition = new InputDefinition("Hello", [], [builtInScheme], false);

        var expectedScheme = new CustomInputScheme()
        {
            DefinitionName = "Hello",
            Name = "Hello",
            DeviceMaps = []
        };

        // Act
        definition.ApplyCustomScheme(expectedScheme);

        var scheme = definition.GetScheme("Hello");

        // Assert
        Assert.NotNull(scheme);
        Assert.Equal(builtInScheme, scheme);
    }

    [Fact]
    public void ApplyCustomScheme_Valid_AddsToDefinition_ReturnsSuccessfully()
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);

        var newScheme = new CustomInputScheme()
        {
            DefinitionName = "Hello",
            Name = "Hello",
            DeviceMaps = []
        };

        // Act
        definition.ApplyCustomScheme(newScheme);

        var scheme = definition.GetScheme("Hello");

        // Assert
        Assert.NotNull(scheme);
        Assert.Equal(newScheme.Name, scheme.Name);
    }


    [Fact]
    public void ApplyCustomScheme_ValidSchemeButCustomSchemeWithSameNameExists_ReplacesSchemeInDefinition_ReturnsSuccessfully()
    {
        // Arrange
        var originalCustomScheme = new InputScheme("Hello", [], false, true);
        var definition = new InputDefinition("Hello", [], [originalCustomScheme], false);

        var expectedScheme = new CustomInputScheme()
        {
            DefinitionName = "Hello",
            Name = "Hello",
            DeviceMaps = []
        };

        // Act
        definition.ApplyCustomScheme(expectedScheme);

        var scheme = definition.GetScheme("Hello");

        // Assert
        Assert.NotNull(scheme);
        Assert.Equal(expectedScheme.Name, scheme.Name);
    }

    #endregion

    #region ResetDefinition

    [Fact]
    public void ResetDefinition_NoSchemes_ReturnsSuccessfully()
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        definition.ResetDefinition();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void ResetDefinition_BuiltInScheme_DoesNotRemove_ReturnsSuccessfully()
    {
        // Arrange
        var expectedScheme = new InputScheme("Hello", [], false, false);
        var definition = new InputDefinition("Hello", [], [expectedScheme], false);

        // Act
        definition.ResetDefinition();

        // Assert
        var scheme = definition.GetScheme("Hello");

        Assert.NotNull(scheme);
        Assert.Equal(expectedScheme, scheme);
    }

    [Fact]
    public void ResetDefinition_BuiltInAndCustomSchemes_OnlyRemovesCustomScheme_ReturnsSuccessfully()
    {
        // Arrange
        var expectedScheme = new InputScheme("Hello", [], false, false);
        var definition = new InputDefinition("Hello", [], [expectedScheme], false);

        var customScheme = new CustomInputScheme() { Name = "What", DefinitionName = "Hello", DeviceMaps = [] };
        definition.ApplyCustomScheme(customScheme);

        Assert.NotNull(definition.GetScheme("What"));

        // Act
        definition.ResetDefinition();

        // Assert
        Assert.Null(definition.GetScheme("What"));

        var scheme = definition.GetScheme("Hello");
        Assert.NotNull(scheme);
        Assert.Equal(expectedScheme, scheme);
    }

    #endregion
}
