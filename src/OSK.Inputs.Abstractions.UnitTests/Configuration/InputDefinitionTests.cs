using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;
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
        var definition = new InputDefinition("Hello", [new InputAction("Hello", new HashSet<InputPhase>(), _ => { }, [])], [], false);

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
    public void GetScheme_InvalidCombinationId_ReturnsNull(string? combinationId)
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        var action = definition.GetScheme(combinationId!, "Abc");

        // Assert
        Assert.Null(action);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetScheme_InvalidSchemeName_ReturnsNull(string? schemeName)
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        var action = definition.GetScheme("Abc", schemeName!);

        // Assert
        Assert.Null(action);
    }

    [Fact]
    public void GetScheme_SchemeNameDoesNotExist_ReturnsNull()
    {
        // Arrange 
        var definition = new InputDefinition("Hello", [], [], false);

        // Act
        var action = definition.GetScheme("Hello", "Hello");

        // Assert
        Assert.Null(action);
    }

    [Fact]
    public void GetScheme_ValidSchemeName_ReturnsAction()
    {
        // Arrange 
        var map = new DeviceInputMap()
        {
            DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad),
            InputMaps = [],
            VirtualMaps = []
        };
        var definition = new InputDefinition("Hello", [], [new InputScheme("Hello", [map], false, false)], false);

        // Act
        var action = definition.GetScheme("Hello", "Hello");

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
            DeviceMaps = [new DeviceInputMap() { DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad), InputMaps = [], VirtualMaps = [] }]
        });

        var scheme = definition.GetSchemesByDevicecCombination("Hello");

        // Assert
        Assert.Empty(scheme);
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
            DeviceMaps = [new DeviceInputMap() { DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad), InputMaps = [], VirtualMaps = [] }]
        });

        var scheme = definition.GetSchemesByDevicecCombination("Hello");

        // Assert
        Assert.Empty(scheme);
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
            DeviceMaps = [new DeviceInputMap() { DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad), InputMaps = [], VirtualMaps = [] }]
        });

        var schemes = definition.GetSchemesByDevicecCombination(schemeName!);

        // Assert
        Assert.Empty(schemes);
    }

    [Fact]
    public void ApplyCustomScheme_ValidSchemeButBuiltInSchemeWithSameNameExists_DoesNotAddToDefinition_ReturnsSuccessfully()
    {
        // Arrange
        var map = new DeviceInputMap()
        {
            DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad),
            InputMaps = [],
            VirtualMaps = []
        };
        var builtInScheme = new InputScheme("Hello", [map], false, false);
        var definition = new InputDefinition("Hello", [], [builtInScheme], false);

        var expectedScheme = new CustomInputScheme()
        {
            DefinitionName = "Hello",
            Name = "Hello",
            DeviceMaps = [map]
        };

        // Act
        definition.ApplyCustomScheme(expectedScheme);

        var scheme = definition.GetScheme("Hello", "HELLO");

        // Assert
        Assert.NotNull(scheme);
        Assert.Equal(builtInScheme, scheme);
    }

    [Fact]
    public void ApplyCustomScheme_Valid_AddsToDefinition_ReturnsSuccessfully()
    {
        // Arrange
        var map = new DeviceInputMap() { DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad), InputMaps = [], VirtualMaps = [] };
        var definition = new InputDefinition("Hello", [], [new InputScheme("Abc", [map], false, false)], false);

        var newScheme = new CustomInputScheme()
        {
            DefinitionName = "Hello",
            Name = "Hello",
            DeviceMaps = [map]
        };

        // Act
        definition.ApplyCustomScheme(newScheme);

        var scheme = definition.GetScheme("Hello", "Hello");

        // Assert
        Assert.NotNull(scheme);
        Assert.Equal(newScheme.Name, scheme.Name);
    }


    [Fact]
    public void ApplyCustomScheme_ValidSchemeButCustomSchemeWithSameNameExists_ReplacesSchemeInDefinition_ReturnsSuccessfully()
    {
        // Arrange
        var map = new DeviceInputMap()
        {
            DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad),
            InputMaps = [],
            VirtualMaps = []
        };
        var originalCustomScheme = new InputScheme("Hello", [map], false, true);
        var definition = new InputDefinition("Hello", [], [originalCustomScheme], false);

        var expectedScheme = new CustomInputScheme()
        {
            DefinitionName = "Hello",
            Name = "Hello",
            DeviceMaps = [ map ]
        };

        // Act
        definition.ApplyCustomScheme(expectedScheme);

        var scheme = definition.GetScheme("Hello", "Hello");

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
        var expectedScheme = new InputScheme("Hello", [new DeviceInputMap() { DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad), InputMaps = [], VirtualMaps = [] }], 
                                             false, false);
        var definition = new InputDefinition("Hello", [], [expectedScheme], false);

        // Act
        definition.ResetDefinition();

        // Assert
        var scheme = definition.GetSchemesByDevicecCombination("Hello");

        Assert.NotNull(scheme);
        Assert.Equal(expectedScheme, scheme.First());
    }

    [Fact]
    public void ResetDefinition_BuiltInAndCustomSchemes_OnlyRemovesCustomScheme_ReturnsSuccessfully()
    {
        // Arrange
        var map = new DeviceInputMap()
        {
            DeviceFamily = new InputDeviceFamily("Hello", InputDeviceType.GamePad),
            InputMaps = [],
            VirtualMaps = []
        };
        var expectedScheme = new InputScheme("Hello", [map],
                                              false, false);
        var definition = new InputDefinition("Hello", [], [expectedScheme], false);

        var customScheme = new CustomInputScheme() { Name = "What", DefinitionName = "Hello", DeviceMaps = [map] };
        definition.ApplyCustomScheme(customScheme);

        Assert.NotEmpty(definition.GetSchemesByDevicecCombination("HeLLO"));

        // Act
        definition.ResetDefinition();

        // Assert
        Assert.Null(definition.GetScheme(map.DeviceFamily.Name, "What"));

        var scheme = definition.GetScheme(map.DeviceFamily.Name, "Hello");
        Assert.NotNull(scheme);
        Assert.Equal(expectedScheme, scheme);
    }

    #endregion
}
