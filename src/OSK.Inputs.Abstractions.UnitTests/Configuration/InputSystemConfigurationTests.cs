using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.UnitTests._Helpers;

namespace OSK.Inputs.Abstractions.UnitTests.Configuration;

public class InputSystemConfigurationTests
{
    #region SupportedDeviceCombinations

    [Fact]
    public void SupportedDeviceCombinations_EmptySchemes_ReturnsEmpty()
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act/Assert
        Assert.Empty(configuration.SupportedDeviceCombinations);
    }

    [Fact]
    public void SupportedDeviceCombinations_VariousSchemesAndDevices_ReturnsExpectedCombinations()
    {
        // Arrange

        // Creates a set of schemes with the following:
        // 1. Identity1
        // 2. Identity1 + Identity2
        // 3. Redudant Identity1
        var definition1 = new InputDefinition("abc",
            [
              new InputScheme("Abc", [new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }], false, false),
              new InputScheme("Def", 
               [
                 new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }, 
                 new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity2, InputMaps = [] }
               ], false, false),
              new InputScheme("GHI", [new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }], false, false)
             ],
            [], false);

        // Creates a set of schemes with the following:
        // 1. Identity1
        // 2. Identity1 + Identity2
        // 3. Identity4
        var definition2 = new InputDefinition("def",
            [
              new InputScheme("Abc", [new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }], false, false),
                      new InputScheme("Def",
                       [
                         new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] },
                         new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity2, InputMaps = [] }
                       ], false, false),
                      new InputScheme("GHI", [new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity4, InputMaps = [] }], false, false)
             ],
            [], false);
        var configuration = new InputSystemConfiguration([], [definition1, definition2], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var supportedDeviceCombinations = configuration.SupportedDeviceCombinations;

        // Assert
        Assert.Equal(3, supportedDeviceCombinations.Count);

        Assert.Single(supportedDeviceCombinations,
            combination => combination.DeviceIdentities.Count is 1 && combination.DeviceIdentities.Contains(TestIdentity.Identity1));
        Assert.Single(supportedDeviceCombinations,
            combination => combination.DeviceIdentities.Count is 2 && combination.DeviceIdentities.Contains(TestIdentity.Identity1)
                && combination.DeviceIdentities.Contains(TestIdentity.Identity2));
        Assert.Single(supportedDeviceCombinations,
            combination => combination.DeviceIdentities.Count is 1 && combination.DeviceIdentities.Contains(TestIdentity.Identity4));
    }

    #endregion

    #region GetDeviceSpecification

    [Fact]
    public void GetDeviceSpecification_DeviceIdentityDoesNotExist_ReturnsNull()
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var specification = configuration.GetDeviceSpecification(TestIdentity.Identity1);

        // Assert
        Assert.Null(specification);
    }


    [Fact]
    public void GetDeviceSpecification_DeviceIdentityExists_ReturnsSpecification()
    {
        // Arrange
        var expectedSpecification = new TestDeviceSpecification();
        var configuration = new InputSystemConfiguration([expectedSpecification], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var specification = configuration.GetDeviceSpecification(TestIdentity.Identity1);

        // Assert
        Assert.NotNull(specification);
        Assert.Equal(expectedSpecification, specification);
    }

    #endregion

    #region GetDefinition

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetDefinition_InvalidDefinitionName_ReturnsNull(string? definitionName)
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var definition = configuration.GetDefinition(definitionName!);

        // Assert
        Assert.Null(definition);
    }

    [Fact]
    public void GetDefinition_DefinitionNameDoesNotExist_ReturnsNull()
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var definition = configuration.GetDefinition("Abc");

        // Assert
        Assert.Null(definition);
    }

    [Fact]
    public void GetDefinition_DefinitionNameExists_ReturnsDefinition()
    {
        // Arrange
        var expectedDefinition = new InputDefinition("Abc", [], [], false);
        var configuration = new InputSystemConfiguration([], [expectedDefinition], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var definition = configuration.GetDefinition("Abc");

        // Assert
        Assert.NotNull(definition);
        Assert.Equal(expectedDefinition, definition);
    }

    #endregion

    #region GetSchemeMap

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetSchemeMap_InvalidDefinitionName_ReturnsNull(string? definitionName)
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var map = configuration.GetSchemeMap(definitionName!, "Abc");

        // Assert
        Assert.Null(map);
    }

    [Fact]
    public void GetSchemeMap_DefinitionNameDoesNotExist_ReturnsNull()
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var definition = configuration.GetSchemeMap("Abc", "Abc");

        // Assert
        Assert.Null(definition);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetSchemeMap_InvalidSchemeName_ReturnsNull(string? schemeName)
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);
        var configuration = new InputSystemConfiguration([], [definition], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var map = configuration.GetSchemeMap(definition.Name, schemeName!);

        // Assert
        Assert.Null(map);
    }

    [Fact]
    public void GetSchemeMap_SchemeNameDoesNotExist_ReturnsNull()
    {
        // Arrange
        var definition = new InputDefinition("Hello", [], [], false);
        var configuration = new InputSystemConfiguration([], [definition], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var map = configuration.GetSchemeMap(definition.Name, "Abc");

        // Assert
        Assert.Null(map);
    }

    [Fact]
    public void GetSchemeMap_Valid_ReturnsSchemeMap()
    {
        // Arrange
        var definition = new InputDefinition("Hello", [new InputScheme("Abc", [], false, false)], [], false);
        var configuration = new InputSystemConfiguration([], [definition], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        var map = configuration.GetSchemeMap(definition.Name, "Abc");

        // Assert
        Assert.NotNull(map);
    }

    #endregion

    #region ApplyCustomInputSchemes

    [Fact]
    public void ApplyCustomInputSchemes_NullCustomSchemes_ReturnsSuccessfully()
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        configuration.ApplyCustomInputSchemes(null!);

        // Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ApplyCustomInputSchemes_InvalidDefinitionName_DoesNotAddScheme_ReturnsSuccessully(string? definitionName)
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        configuration.ApplyCustomInputSchemes([ new CustomInputScheme() { DefinitionName = definitionName!, Name = "Abc", DeviceMaps = [] }]);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void ApplyCustomInputSchemes_DefinitionNameDoesNotExist_ReturnsSuccessfully()
    {
        // Arrange
        var configuration = new InputSystemConfiguration([], [], new InputProcessorConfiguration(), new InputSystemJoinPolicy());

        // Act
        configuration.ApplyCustomInputSchemes([ new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] }]);

        // Assert
        Assert.True(true);
    }

    #endregion
}
