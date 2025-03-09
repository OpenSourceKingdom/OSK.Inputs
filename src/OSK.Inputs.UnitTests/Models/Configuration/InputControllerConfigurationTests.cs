using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using Xunit;

namespace OSK.Inputs.UnitTests.Models.Configuration;

public class InputControllerConfigurationTests
{
    #region GetActiveScheme

    [Fact]
    public void GetActiveScheme_EmptyInputSchemes_ThrowsInvalidOperationException()
    {
        // Arrange
        var controller = GetInputControllerConfiguration();

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => controller.GetActiveScheme(null));
    }

    [Fact]
    public void GetActiveScheme_NoPreferredActiveScheme_ConfigurationHasDefaultScheme_ReturnsDefault()
    {
        // Arrange
        var scheme1 = new InputScheme("abc", "abc", "abc", [], false);
        var scheme2 = new InputScheme("abc", "abc", "def", [], false);
        var scheme3 = new InputScheme("abc", "abc", "ghi", [], true);
        var controller = GetInputControllerConfiguration(scheme1, scheme2, scheme3);

        // Act
        var activeScheme = controller.GetActiveScheme(null);

        // Assert
        Assert.Equal(scheme3, activeScheme);
    }

    [Fact]
    public void GetActiveScheme_NoPreferredActiveScheme_ConfigurationHasMultipleDefaultSchemes_ReturnsLastDefault()
    {
        // Arrange
        var scheme1 = new InputScheme("abc", "abc", "abc", [], true);
        var scheme2 = new InputScheme("abc", "abc", "def", [], true);
        var scheme3 = new InputScheme("abc", "abc", "ghi", [], true);
        var controller = GetInputControllerConfiguration(scheme1, scheme2, scheme3);

        // Act
        var activeScheme = controller.GetActiveScheme(null);

        // Assert
        Assert.Equal(scheme3, activeScheme);
    }

    [Fact]
    public void GetActiveScheme_NoPreferredActiveScheme_ConfigurationHasNoDefaultScheme_ReturnsFirstScheme()
    {
        // Arrange
        var scheme1 = new InputScheme("abc", "abc", "abc", [], false);
        var scheme2 = new InputScheme("abc", "abc", "def", [], false);
        var scheme3 = new InputScheme("abc", "abc", "ghi", [], false);
        var controller = GetInputControllerConfiguration(scheme1, scheme2, scheme3);

        // Act
        var activeScheme = controller.GetActiveScheme(null);

        // Assert
        Assert.Equal(scheme1, activeScheme);
    }

    [Fact]
    public void GetActiveScheme_PreferredActiveScheme_ConfigurationHasNoMatchingScheme_DefaultSchemeSet_ReturnsDefault()
    {
        // Arrange
        var scheme1 = new InputScheme("abc", "abc", "abc", [], false);
        var scheme2 = new InputScheme("abc", "abc", "def", [], true);
        var scheme3 = new InputScheme("abc", "abc", "ghi", [], false);
        var controller = GetInputControllerConfiguration(scheme1, scheme2, scheme3);

        // Act
        var activeScheme = controller.GetActiveScheme(new ActiveInputScheme(1 ,"abc", "abc", "NoWay"));

        // Assert
        Assert.Equal(scheme2, activeScheme);
    }


    [Fact]
    public void GetActiveScheme_PreferredActiveScheme_ConfigurationHasMatchingScheme_DefaultSchemeSet_ReturnsPreferredScheme()
    {
        // Arrange
        var scheme1 = new InputScheme("abc", "abc", "abc", [], false);
        var scheme2 = new InputScheme("abc", "abc", "def", [], true);
        var scheme3 = new InputScheme("abc", "abc", "ghi", [], false);
        var controller = GetInputControllerConfiguration(scheme1, scheme2, scheme3);

        // Act
        var activeScheme = controller.GetActiveScheme(new ActiveInputScheme(1, "abc", "abc", scheme3.SchemeName));

        // Assert
        Assert.Equal(scheme3, activeScheme);
    }

    #endregion

    #region Clone

    #endregion

    #region Helpers

    private InputControllerConfiguration GetInputControllerConfiguration(params InputScheme[] schemes)
        => new InputControllerConfiguration("abc", [], schemes);

    #endregion
}
