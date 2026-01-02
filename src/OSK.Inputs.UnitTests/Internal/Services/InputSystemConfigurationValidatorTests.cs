using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputSystemConfigurationValidatorTests
{
    #region Validate

    [Fact]
    public void Validate_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    

    #endregion

    #region ValidateCustomScheme

    [Fact]
    public void ValidateCustomScheme_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => validator.ValidateCustomScheme(null!, new CustomInputScheme()
        {
            DefinitionName = "Abc",
            Name = "Name",
            DeviceMaps = []
        }, false));
    }

    [Fact]
    public void ValidateCustomScheme_NullScheme_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => validator.ValidateCustomScheme(new InputSystemConfiguration([], [], new(), new()), null!, false));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateCustomScheme_InvalidDefinitionName_ReturnsError(string? definitionName)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], [], new(), new()),
            new CustomInputScheme() { DefinitionName = definitionName!, Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DefinitionNameNotFound_ReturnsError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], [], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
        Assert.Equal(InputConfigurationType.Definition, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateCustomScheme_InvalidSchemeName_ReturnsError(string? schemeName)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], [new InputDefinition("Abc", [], [], false)], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = schemeName!, DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeBuiltIn_DontSkipDuplicateCustomSchemeGuard_ReturnsDuplicateError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], 
            [
                new InputDefinition("Abc", 
                    [
                        new InputScheme("Abc", [], false, false)
                    ], [], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeBuiltIn_SkipDuplicateCustomSchemeGuard_ReturnsDuplicateError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, false)
                    ], [], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            true);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeCustom_DontSkipDuplicateCustomSchemeGuard_ReturnsDuplicateError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, true)
                    ], [], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeCustom_SkipDuplicateCustomSchemeGuard_InvalidDeviceMaps_ReturnsError(bool useEmpty)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, true)
                    ], [], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            true);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal(nameof(CustomInputScheme.DeviceMaps), validation.TargetName);
    }

    #endregion
}
